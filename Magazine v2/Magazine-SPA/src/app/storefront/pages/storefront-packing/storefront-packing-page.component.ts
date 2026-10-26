import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { StorefrontPackingAllocation, StorefrontPackingOrder, StorefrontPoolOrder } from '../../models/storefront.models';
import { StorefrontPackingService } from '../../services/storefront-packing.service';
import { formatPrintDate, printDocument } from '../../../shared/print/print-document';

@Component({
  selector: 'app-storefront-packing-page',
  imports: [
    DatePipe,
    DecimalPipe,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './storefront-packing-page.component.html',
  styleUrls: ['../../../shared/styles/management-page.scss', './storefront-packing-page.component.scss'],
})
export class StorefrontPackingPageComponent implements OnInit {
  private readonly service = inject(StorefrontPackingService);
  private readonly account = inject(AccountService);
  private readonly notification = inject(NotificationService);

  readonly displayedColumns = [
    'number',
    'customer',
    'warehouse',
    'createdOnUtc',
    'items',
    'totalValue',
    'status',
    'actions',
  ];
  readonly orders = signal<StorefrontPackingOrder[]>([]);
  readonly pool = signal<StorefrontPoolOrder[]>([]);
  readonly poolColumns = ['number', 'customer', 'items', 'totalValue', 'actions'];
  readonly loading = signal(false);
  readonly processing = signal<string | null>(null);
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) this.load();
    });
  }

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin([this.account.load(), this.account.loadPermissions()]).subscribe({
      next: () => {
        this.initialized = true;
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.initialized = true;
        this.loading.set(false);
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać danych konta.');
      },
    });
  }

  get canPack(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('shipments.approve')
    );
  }

  load(): void {
    this.loading.set(true);
    this.service.getPool().subscribe({
      next: (pool) => this.pool.set(pool),
      error: () => this.pool.set([]),
    });
    this.service.getPackingOrders().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać zakupów do spakowania.');
      },
    });
  }

  claim(row: StorefrontPoolOrder): void {
    if (!this.canPack || this.processing()) return;
    this.processing.set(row.id);
    this.service.claim(row.id).subscribe({
      next: () => {
        this.processing.set(null);
        this.notification.success('Zamówienie przyjęte przez Twój magazyn. Towar został zarezerwowany.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.showActionError(error);
        this.load();
      },
    });
  }

  accept(row: StorefrontPackingOrder): void {
    if (!this.canPack || this.processing()) return;
    this.processing.set(row.id);
    this.service.accept(row.id).subscribe({
      next: () => {
        this.processing.set(null);
        this.notification.success('Zamówienie przyjęte do kompletacji.');
        this.load();
      },
      error: (error: HttpErrorResponse) => this.showActionError(error),
    });
  }

  pack(row: StorefrontPackingOrder): void {
    if (!this.canPack || this.processing()) return;
    this.processing.set(row.id);
    this.service.pack(row.id).subscribe({
      next: () => {
        this.processing.set(null);
        this.notification.success('Zamówienie spakowane i zdjęte ze stanu.');
        this.load();
      },
      error: (error: HttpErrorResponse) => this.showActionError(error),
    });
  }

  printDocument(row: StorefrontPackingOrder): void {
    printDocument({
      title: `Pakowanie ${row.number}`,
      heading: `Lista kompletacji ${row.number}`,
      meta: [
        ['Miejsce wystawienia', row.assignedWarehouseName],
        ['Data wystawienia', formatPrintDate(new Date())],
      ],
      parties: [
        { title: 'Magazyn', lines: [row.assignedWarehouseName] },
        { title: 'Klient', lines: [row.customerName, row.deliveryAddress, row.customerEmail] },
      ],
      columns: [
        { label: 'Lp.', width: '36px', align: 'center' },
        { label: 'Nazwa towaru' },
        { label: 'SKU' },
        { label: 'Ilość', align: 'right', width: '60px' },
        { label: 'Lokalizacje' },
      ],
      rows: row.items.map((item, index) => [
        index + 1,
        item.productName,
        item.sku,
        item.quantity,
        this.allocationLabel(item.allocations),
      ]),
      signatures: [{ title: 'Spakował(a):', caption: 'Podpis osoby pakującej' }],
    });
  }

  allocationLabel(allocations: StorefrontPackingAllocation[]): string {
    return allocations
      .map((allocation) => `${allocation.locationCode}: ${allocation.quantity}`)
      .join(', ');
  }

  private showActionError(error: HttpErrorResponse): void {
    this.processing.set(null);
    this.notification.error(error.error?.message ?? 'Nie udało się wykonać operacji.');
  }
}
