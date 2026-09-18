import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';
import JsBarcode from 'jsbarcode';
import QRCode from 'qrcode';
import { forkJoin } from 'rxjs';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { SelectOption, Shipment, ShipmentPageData } from '../../models/management.models';
import { ShipmentHistoryModalComponent } from '../../modals/shipment-history/shipment-history-modal.component';
import { ShipmentReceiveModalComponent } from '../../modals/shipment-receive/shipment-receive-modal.component';
import { ShipmentModalComponent } from '../../modals/shipment/shipment-modal.component';
import { ShipmentService } from '../../services/shipment.service';

interface ShipmentPrintCodes {
  qr: string;
  barcode: string;
}

@Component({
  selector: 'app-shipments-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './shipments-page.component.html',
  styleUrls: ['../management-page.scss', './shipments-page.component.scss'],
})
export class ShipmentsPageComponent implements OnInit {
  private readonly service = inject(ShipmentService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);

  readonly displayedColumns = ['number', 'route', 'createdOnUtc', 'items', 'status', 'actions'];
  readonly mode = this.route.snapshot.data['mode'] as 'all' | 'pending' | 'ready';
  readonly shipments = signal<Shipment[]>([]);
  readonly readyShipments = signal<Shipment[]>([]);
  readonly pageData = signal<ShipmentPageData | null>(null);
  readonly loading = signal(false);
  readonly loadingReady = signal(false);
  readonly openingMode = signal<'create' | 'request' | null>(null);
  readonly approvingShipmentId = signal('');
  readonly inTransitShipmentId = signal('');
  readonly historyShipmentId = signal('');
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) this.load();
    });
  }

  ngOnInit(): void {
    this.loading.set(this.mode !== 'ready');
    this.loadingReady.set(this.mode === 'ready');
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
      this.account.permissionCodes().includes('shipments.create')
    );
  }

  get canApprove(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('shipments.approve')
    );
  }

  get destinationOptions(): SelectOption[] {
    return (
      this.pageData()?.destinationWarehouses.map((warehouse) => ({
        value: warehouse.id,
        label: warehouse.name,
      })) ?? []
    );
  }

  get pendingShipments(): Shipment[] {
    return this.shipments();
  }

  get visibleShipments(): Shipment[] {
    if (this.mode === 'pending') return this.pendingShipments;
    if (this.mode === 'ready') return this.readyShipments();
    return this.shipments();
  }

  get tableLoading(): boolean {
    return this.loading() || (this.mode === 'ready' && this.loadingReady());
  }

  get moduleTitle(): string {
    if (this.mode === 'pending') return 'Oczekujące na akceptację';
    if (this.mode === 'ready') return 'Wysłane i w drodze';
    return 'Wszystkie wysyłki';
  }

  load(): void {
    this.loading.set(this.mode !== 'ready');
    this.loadingReady.set(this.mode === 'ready');

    const request =
      this.mode === 'ready'
        ? this.service.getReady()
        : this.mode === 'pending'
          ? this.service.getPending()
          : this.service.getAll();

    request.subscribe({
      next: (rows) => {
        if (this.mode === 'ready') this.readyShipments.set(rows);
        else this.shipments.set(rows);
        this.loading.set(false);
        this.loadingReady.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.showError(error);
      },
    });
  }

  add(): void {
    if (this.openingMode()) return;
    if (!this.canCreate) {
      this.notification.error('Brak uprawnień do tworzenia wysyłek.');
      return;
    }
    if (!this.pageData()) {
      this.loadPageDataAndOpen('create', 'Wysyłka została utworzona.');
      return;
    }

    this.openShipmentModal('create')
      .afterClosed()
      .subscribe((created) => {
        if (!created) return;
        this.notification.success('Wysyłka została utworzona.');
        this.load();
      });
  }

  requestShipment(): void {
    if (this.openingMode()) return;
    if (!this.canCreate) {
      this.notification.error('Brak uprawnień do tworzenia wysyłek.');
      return;
    }
    if (!this.pageData()) {
      this.loadPageDataAndOpen('request', 'Prośba o wysyłkę została wysłana.');
      return;
    }

    this.openShipmentModal('request')
      .afterClosed()
      .subscribe((created) => {
        if (!created) return;
        this.notification.success('Prośba o wysyłkę została wysłana.');
        this.load();
      });
  }

  private loadPageDataAndOpen(mode: 'create' | 'request', successMessage: string): void {
    this.openingMode.set(mode);
    this.service.getPageData().subscribe({
      next: (data) => {
        this.pageData.set(data);
        this.openingMode.set(null);
        this.openShipmentModal(mode)
          .afterClosed()
          .subscribe((created) => {
            if (!created) return;
            this.notification.success(successMessage);
            this.load();
          });
      },
      error: () => {
        this.openingMode.set(null);
        this.notification.error('Nie udało się pobrać danych do wysyłki.');
      },
    });
  }

  private openShipmentModal(mode: 'create' | 'request') {
    const pageData = this.pageData();
    if (!pageData) throw new Error('Shipment page data is not loaded.');

    return this.dialog.open(ShipmentModalComponent, {
      width: '820px',
      maxWidth: '96vw',
      data: {
        sourceWarehouseName: pageData.sourceWarehouse.name,
        destinations: this.destinationOptions,
        availableProducts: pageData.availableProducts,
        mode,
      },
    });
  }

  approve(row: Shipment): void {
    if (!this.canApprove || row.status !== 'Oczekuje na akceptację') return;
    this.approvingShipmentId.set(row.id);
    this.service.approve(row.id).subscribe({
      next: () => {
        this.approvingShipmentId.set('');
        this.notification.success('Wysyłka została wysłana.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.approvingShipmentId.set('');
        this.showError(error);
      },
    });
  }

  markInTransit(row: Shipment): void {
    if (!this.canApprove || row.status !== 'Wysłana') return;
    this.inTransitShipmentId.set(row.id);
    this.service.markInTransit(row.id).subscribe({
      next: () => {
        this.inTransitShipmentId.set('');
        this.notification.success('Wysyłka oznaczona jako w drodze.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.inTransitShipmentId.set('');
        this.showError(error);
      },
    });
  }

  canReceive(row: Shipment): boolean {
    return (
      this.canApprove &&
      row.status === 'W drodze' &&
      (this.account.user()?.roleId === administratorRoleId ||
        row.destinationWarehouseId === this.account.activeWarehouseId())
    );
  }

  receive(row?: Shipment): void {
    if (row && !this.canReceive(row)) return;

    this.dialog
      .open(ShipmentReceiveModalComponent, {
        width: '820px',
        maxWidth: '96vw',
        data: { identifier: row?.number },
      })
      .afterClosed()
      .subscribe((received) => {
        if (!received) return;
        this.notification.success('Wysyłka odebrana i przyjęta na stan.');
        this.load();
      });
  }

  showHistory(row: Shipment): void {
    this.historyShipmentId.set(row.id);
    this.service.getHistory(row.id).subscribe({
      next: (history) => {
        this.historyShipmentId.set('');
        this.dialog.open(ShipmentHistoryModalComponent, {
          width: '760px',
          maxWidth: '96vw',
          data: { shipment: row, history },
        });
      },
      error: (error: HttpErrorResponse) => {
        this.historyShipmentId.set('');
        this.showError(error);
      },
    });
  }

  printWz(row: Shipment): void {
    void this.printShipmentDocument(row, 'wz');
  }

  printLabel(row: Shipment): void {
    void this.printShipmentDocument(row, 'label');
  }

  private showError(error: HttpErrorResponse): void {
    this.loading.set(false);
    this.loadingReady.set(false);
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać wysyłek.');
  }

  private async printShipmentDocument(row: Shipment, type: 'wz' | 'label'): Promise<void> {
    try {
      const codes = await this.generateShipmentCodes(row.id);
      const title = type === 'wz' ? `WZ ${row.number}` : `Naklejka ${row.number}`;
      const body = type === 'wz' ? this.wzDocument(row, codes) : this.labelDocument(row, codes);
      this.openPrintWindow(title, body);
    } catch {
      this.notification.error('Nie udało się wygenerować kodów do druku.');
    }
  }

  private openPrintWindow(title: string, body: string): void {
    const printWindow = window.open('', '_blank', 'width=900,height=700');
    if (!printWindow) return;
    const safeTitle = this.escapeHtml(title);
    printWindow.document.write(`
      <html>
        <head>
          <title>${safeTitle}</title>
          <style>
            body { font-family: Arial, sans-serif; color: #111; margin: 32px; }
            header { display: flex; justify-content: space-between; gap: 24px; align-items: flex-start; }
            table { border-collapse: collapse; width: 100%; margin-top: 24px; }
            th, td { border: 1px solid #999; padding: 8px; text-align: left; }
            .code-panel { text-align: center; min-width: 190px; }
            .code-panel img { display: block; margin: 0 auto 8px; }
            .barcode { width: 260px; max-width: 100%; }
            .label { border: 2px solid #111; width: 420px; padding: 20px; }
            .label h2 { margin: 0 0 8px; }
            .label-code { text-align: center; margin: 18px 0; }
            .label-code img { display: block; margin: 0 auto 10px; }
            .muted { color: #555; }
            .identifier { font-size: 11px; overflow-wrap: anywhere; }
          </style>
        </head>
        <body>${body}<script>window.addEventListener('load', function () { setTimeout(function () { window.print(); }, 150); });</script></body>
      </html>
    `);
    printWindow.document.close();
  }

  private wzDocument(row: Shipment, codes: ShipmentPrintCodes): string {
    const items = row.items
      .map(
        (item, index) => `
          <tr>
            <td>${index + 1}</td>
            <td>${this.escapeHtml(item.productName)}</td>
            <td>${this.escapeHtml(item.sku)}</td>
            <td>${this.escapeHtml(item.barcode)}</td>
            <td>${item.quantity}</td>
          </tr>
        `,
      )
      .join('');

    return `
      <header>
        <div>
          <h1>WZ ${this.escapeHtml(row.number)}</h1>
          <p><strong>Skąd:</strong> ${this.escapeHtml(row.sourceWarehouseName)}</p>
          <p><strong>Dokąd:</strong> ${this.escapeHtml(row.destinationWarehouseName)}</p>
          <p><strong>Data akceptacji:</strong> ${this.formatDate(row.approvedOnUtc)}</p>
        </div>
        <div class="code-panel">
          <img src="${codes.qr}" width="132" height="132" alt="QR wysyłki" />
          <img class="barcode" src="${codes.barcode}" alt="Kod kreskowy wysyłki" />
          <div class="identifier">${this.escapeHtml(row.id)}</div>
        </div>
      </header>
      <table>
        <thead>
          <tr><th>Lp.</th><th>Produkt</th><th>SKU</th><th>Kod</th><th>Ilość</th></tr>
        </thead>
        <tbody>${items}</tbody>
      </table>
    `;
  }

  private labelDocument(row: Shipment, codes: ShipmentPrintCodes): string {
    return `
      <div class="label">
        <h2>${this.escapeHtml(row.number)}</h2>
        <p class="muted">Wysyłka magazynowa</p>
        <div class="label-code">
          <img src="${codes.qr}" width="180" height="180" alt="QR wysyłki" />
          <img class="barcode" src="${codes.barcode}" alt="Kod kreskowy wysyłki" />
          <div class="identifier">${this.escapeHtml(row.id)}</div>
        </div>
        <p><strong>Skąd:</strong><br />${this.escapeHtml(row.sourceWarehouseName)}</p>
        <p><strong>Dokąd:</strong><br />${this.escapeHtml(row.destinationWarehouseName)}</p>
        <p><strong>Pozycji:</strong> ${row.items.length}</p>
      </div>
    `;
  }

  private async generateShipmentCodes(identifier: string): Promise<ShipmentPrintCodes> {
    const qr = await QRCode.toDataURL(identifier, {
      errorCorrectionLevel: 'M',
      margin: 1,
      width: 220,
    });

    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    JsBarcode(svg, identifier, {
      format: 'CODE128',
      displayValue: false,
      height: 58,
      margin: 0,
      width: 1.35,
    });

    return {
      qr,
      barcode: this.svgToDataUrl(svg),
    };
  }

  private svgToDataUrl(svg: SVGSVGElement): string {
    const serialized = new XMLSerializer().serializeToString(svg);
    const encoded = window.btoa(unescape(encodeURIComponent(serialized)));
    return `data:image/svg+xml;base64,${encoded}`;
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

  private formatDate(value: string | null): string {
    if (!value) return '';
    return new Intl.DateTimeFormat('pl-PL', {
      dateStyle: 'short',
      timeStyle: 'short',
    }).format(new Date(value));
  }
}
