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
import {
  StorefrontPackingAllocation,
  StorefrontPackingOrder,
} from '../../models/management.models';
import { StorefrontPackingService } from '../../services/storefront-packing.service';

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
  styleUrls: ['../management-page.scss', './storefront-packing-page.component.scss'],
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
    const printWindow = window.open('', '_blank', 'width=900,height=700');
    if (!printWindow) return;

    const items = row.items
      .map(
        (item, index) => `
          <tr>
            <td>${index + 1}</td>
            <td>${this.escapeHtml(item.productName)}</td>
            <td>${this.escapeHtml(item.sku)}</td>
            <td>${item.quantity}</td>
            <td>${this.escapeHtml(item.allocations.map((allocation) => `${allocation.locationCode}: ${allocation.quantity}`).join(', '))}</td>
          </tr>
        `,
      )
      .join('');

    printWindow.document.write(`
      <html>
        <head>
          <title>${this.escapeHtml(row.number)}</title>
          <style>
            body { font-family: Arial, sans-serif; color: #111; margin: 32px; }
            table { border-collapse: collapse; width: 100%; margin-top: 24px; }
            th, td { border: 1px solid #999; padding: 8px; text-align: left; }
            .meta { display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px 24px; }
          </style>
        </head>
        <body>
          <h1>Pakowanie ${this.escapeHtml(row.number)}</h1>
          <div class="meta">
            <p><strong>Magazyn:</strong> ${this.escapeHtml(row.assignedWarehouseName)}</p>
            <p><strong>Klient:</strong> ${this.escapeHtml(row.customerName)}</p>
            <p><strong>Email:</strong> ${this.escapeHtml(row.customerEmail)}</p>
            <p><strong>Adres:</strong> ${this.escapeHtml(row.deliveryAddress ?? '')}</p>
          </div>
          <table>
            <thead>
              <tr><th>Lp.</th><th>Produkt</th><th>SKU</th><th>Ilość</th><th>Lokalizacje</th></tr>
            </thead>
            <tbody>${items}</tbody>
          </table>
          <script>window.addEventListener('load', function () { setTimeout(function () { window.print(); }, 150); });</script>
        </body>
      </html>
    `);
    printWindow.document.close();
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

  private escapeHtml(value: string): string {
    return value.replace(/[&<>"']/g, (character) => {
      const entities: Record<string, string> = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#39;',
      };
      return entities[character];
    });
  }
}
