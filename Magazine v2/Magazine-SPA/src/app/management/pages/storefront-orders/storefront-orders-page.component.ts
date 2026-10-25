import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { StorefrontOrderHistory } from '../../models/management.models';
import { StorefrontPackingService } from '../../services/storefront-packing.service';
import { StorefrontOrderDetailsModalComponent } from './storefront-order-details-modal.component';
import { StorefrontOrderShipModalComponent } from './storefront-order-ship-modal.component';
import { printSalesDocument } from './print-sales-document';

@Component({
  selector: 'app-storefront-orders-page',
  imports: [
    DatePipe,
    DecimalPipe,
    FormsModule,
    MatButtonModule,
    MatProgressBarModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './storefront-orders-page.component.html',
  styleUrls: ['../management-page.scss', './storefront-orders-page.component.scss'],
})
export class StorefrontOrdersPageComponent implements OnInit {
  private readonly service = inject(StorefrontPackingService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  readonly displayedColumns = ['number', 'customer', 'status', 'handling', 'carrier', 'totalValue', 'actions'];
  readonly statusOptions = [
    { value: null, label: 'Wszystkie' },
    { value: 1, label: 'Nowe' },
    { value: 2, label: 'Przyjęte do realizacji' },
    { value: 3, label: 'Spakowane' },
    { value: 5, label: 'Wysłane' },
    { value: 4, label: 'Anulowane' },
  ];
  readonly periods = [30, 90, 365];

  readonly orders = signal<StorefrontOrderHistory[]>([]);
  readonly loading = signal(false);
  status: number | null = null;
  days = 90;
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

  get canShip(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('shipments.approve')
    );
  }

  load(): void {
    this.loading.set(true);
    this.service.getHistory(this.status, this.days).subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać zamówień sklepu.');
      },
    });
  }

  details(row: StorefrontOrderHistory): void {
    this.dialog.open(StorefrontOrderDetailsModalComponent, {
      width: '760px',
      maxWidth: '96vw',
      data: { order: row },
    });
  }

  ship(row: StorefrontOrderHistory): void {
    this.dialog
      .open(StorefrontOrderShipModalComponent, { width: '480px', maxWidth: '96vw', data: { order: row } })
      .afterClosed()
      .subscribe((shipped) => {
        if (!shipped) return;
        this.notification.success('Zamówienie przekazane przewoźnikowi.');
        this.load();
      });
  }

  printDocument(row: StorefrontOrderHistory): void {
    printSalesDocument(row);
  }
}
