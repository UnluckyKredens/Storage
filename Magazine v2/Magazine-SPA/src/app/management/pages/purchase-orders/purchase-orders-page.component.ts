import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { PurchaseOrder, PurchaseOrderPageData } from '../../models/management.models';
import { PurchaseOrderApproveModalComponent } from '../../modals/purchase-order-approve/purchase-order-approve-modal.component';
import { PurchaseOrderReceiveModalComponent } from '../../modals/purchase-order-receive/purchase-order-receive-modal.component';
import { PurchaseOrderModalComponent } from '../../modals/purchase-order/purchase-order-modal.component';
import { PurchaseOrderService } from '../../services/purchase-order.service';

@Component({
  selector: 'app-purchase-orders-page',
  imports: [
    DatePipe,
    DecimalPipe,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './purchase-orders-page.component.html',
  styleUrls: ['../management-page.scss', './purchase-orders-page.component.scss'],
})
export class PurchaseOrdersPageComponent implements OnInit {
  private readonly service = inject(PurchaseOrderService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  readonly displayedColumns = [
    'number',
    'contractorName',
    'createdOnUtc',
    'documents',
    'items',
    'totalValue',
    'status',
    'actions',
  ];
  readonly orders = signal<PurchaseOrder[]>([]);
  readonly pageData = signal<PurchaseOrderPageData | null>(null);
  readonly loading = signal(false);
  readonly opening = signal(false);
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) {
        this.pageData.set(null);
        this.load();
      }
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
        this.showError(error);
      },
    });
  }

  get canCreate(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('purchase-orders.create')
    );
  }

  get canApprove(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('purchase-orders.approve')
    );
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => this.showError(error),
    });
  }

  add(): void {
    if (this.opening()) return;
    if (!this.canCreate) {
      this.notification.error('Brak uprawnień do tworzenia zamówień.');
      return;
    }

    const data = this.pageData();
    if (data) {
      this.openCreateModal(data);
      return;
    }

    this.opening.set(true);
    this.service.getPageData().subscribe({
      next: (pageData) => {
        this.pageData.set(pageData);
        this.opening.set(false);
        this.openCreateModal(pageData);
      },
      error: (error: HttpErrorResponse) => {
        this.opening.set(false);
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać danych formularza.');
      },
    });
  }

  approve(row: PurchaseOrder): void {
    if (!this.canApprove || row.status !== 'Oczekuje na akceptację') return;

    this.dialog
      .open(PurchaseOrderApproveModalComponent, {
        width: '560px',
        maxWidth: '96vw',
        data: { order: row },
      })
      .afterClosed()
      .subscribe((approved) => {
        if (!approved) return;
        this.notification.success('Zamówienie zostało zaakceptowane.');
        this.load();
      });
  }

  receive(row: PurchaseOrder): void {
    if (!this.canApprove || row.status !== 'Zaakceptowane') return;

    this.dialog
      .open(PurchaseOrderReceiveModalComponent, {
        width: '760px',
        maxWidth: '96vw',
        data: { order: row },
      })
      .afterClosed()
      .subscribe((received) => {
        if (!received) return;
        this.notification.success('Zamówienie przyjęte na stan.');
        this.load();
      });
  }

  printDocument(row: PurchaseOrder): void {
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
            <td>${item.unitPrice.toFixed(2)}</td>
            <td>${item.totalPrice.toFixed(2)}</td>
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
          <h1>Zamówienie ${this.escapeHtml(row.number)}</h1>
          <div class="meta">
            <p><strong>Magazyn:</strong> ${this.escapeHtml(row.warehouseName)}</p>
            <p><strong>Dostawca:</strong> ${this.escapeHtml(row.contractorName)}</p>
            <p><strong>Faktura:</strong> ${this.escapeHtml(row.invoiceNumber ?? '')}</p>
            <p><strong>Dokument papierowy:</strong> ${this.escapeHtml(row.paperDocumentNumber ?? '')}</p>
          </div>
          <table>
            <thead>
              <tr><th>Lp.</th><th>Produkt</th><th>SKU</th><th>Ilość</th><th>Cena</th><th>Wartość</th></tr>
            </thead>
            <tbody>${items}</tbody>
          </table>
          <script>window.addEventListener('load', function () { setTimeout(function () { window.print(); }, 150); });</script>
        </body>
      </html>
    `);
    printWindow.document.close();
  }

  private openCreateModal(pageData: PurchaseOrderPageData): void {
    if (pageData.suppliers.length === 0) {
      this.notification.error('Brak dostawców. Dodaj kontrahenta typu Dostawca.');
      return;
    }
    if (pageData.products.length === 0) {
      this.notification.error('Brak aktywnych produktów do zamówienia.');
      return;
    }

    this.dialog
      .open(PurchaseOrderModalComponent, {
        width: '900px',
        maxWidth: '96vw',
        data: { pageData },
      })
      .afterClosed()
      .subscribe((created) => {
        if (!created) return;
        this.notification.success('Zamówienie zostało utworzone.');
        this.load();
      });
  }

  private showError(error: HttpErrorResponse): void {
    this.loading.set(false);
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać zamówień.');
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
