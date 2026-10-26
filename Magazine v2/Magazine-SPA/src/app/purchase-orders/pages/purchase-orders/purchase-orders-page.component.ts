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
import { PurchaseOrder, PurchaseOrderPageData } from '../../models/purchase-orders.models';
import { PurchaseOrderApproveModalComponent } from '../../modals/purchase-order-approve/purchase-order-approve-modal.component';
import { PurchaseOrderReceiveModalComponent } from '../../modals/purchase-order-receive/purchase-order-receive-modal.component';
import { PurchaseOrderModalComponent } from '../../modals/purchase-order/purchase-order-modal.component';
import { PurchaseOrderService } from '../../services/purchase-order.service';
import {
  formatPrintDate,
  formatPrintMoney,
  printDocument,
  taxLine,
} from '../../../shared/print/print-document';

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
  styleUrls: ['../../../shared/styles/management-page.scss', './purchase-orders-page.component.scss'],
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
    printDocument({
      title: `Zamówienie ${row.number}`,
      heading: `Zamówienie zewnętrzne ${row.number}`,
      meta: [
        ['Miejsce wystawienia', row.warehouseName],
        ['Data wystawienia', formatPrintDate(row.createdOnUtc)],
        ['Data zatwierdzenia', formatPrintDate(row.approvedOnUtc)],
        ['Data przyjęcia', formatPrintDate(row.receivedOnUtc)],
      ],
      parties: [
        { title: 'Zamawiający', lines: [row.warehouseName, row.warehouseAddress, taxLine(row.warehouseTaxNumber)] },
        {
          title: 'Dostawca',
          lines: [
            row.contractorName,
            row.contractorAddress,
            taxLine(row.contractorTaxNumber),
            row.contractorEmail,
            row.contractorPhone,
          ],
        },
      ],
      columns: [
        { label: 'Lp.', width: '36px', align: 'center' },
        { label: 'Nazwa towaru' },
        { label: 'SKU' },
        { label: 'Ilość', align: 'right', width: '60px' },
        { label: 'Cena', align: 'right', width: '80px' },
        { label: 'Wartość', align: 'right', width: '90px' },
      ],
      rows: row.items.map((item, index) => [
        index + 1,
        item.productName,
        item.sku,
        item.quantity,
        formatPrintMoney(item.unitPrice),
        formatPrintMoney(item.totalPrice),
      ]),
      summary: [['Razem', `${formatPrintMoney(row.totalValue)} zł`]],
      notes: [
        ['Faktura', row.invoiceNumber],
        ['Dokument papierowy', row.paperDocumentNumber],
        ['Uwagi', row.notes],
      ],
      signatures: [
        {
          title: 'Zamawiający:',
          caption: 'Podpis osoby upoważnionej do zamówienia',
          signer: row.approvedBy ?? row.createdBy,
          signedOn: row.approvedOnUtc ?? row.createdOnUtc,
        },
        {
          title: 'Przyjął(a):',
          caption: 'Podpis przyjmującego',
          signer: row.receivedBy,
          signedOn: row.receivedOnUtc,
        },
        { title: 'Dostarczył(a):', caption: 'Podpis dostawcy' },
      ],
    });
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
}
