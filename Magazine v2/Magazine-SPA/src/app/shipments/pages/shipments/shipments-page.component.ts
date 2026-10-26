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
import { forkJoin, Observable } from 'rxjs';
import {
  escapeHtml,
  formatPrintDate,
  openPrintWindow,
  printDocument,
  taxLine,
} from '../../../shared/print/print-document';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { SelectOption } from '../../../management/models/management.models';
import { Shipment, ShipmentPageData } from '../../models/shipments.models';
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
  styleUrls: ['../../../shared/styles/management-page.scss', './shipments-page.component.scss'],
})
export class ShipmentsPageComponent implements OnInit {
  private readonly service = inject(ShipmentService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);

  readonly maxVisibleItems = 3;
  readonly displayedColumns = [
    'number',
    'route',
    'createdOnUtc',
    'items',
    'documents',
    'status',
    'actions',
  ];
  readonly mode = this.route.snapshot.data['mode'] as 'all' | 'pending' | 'ready';
  readonly shipments = signal<Shipment[]>([]);
  readonly readyShipments = signal<Shipment[]>([]);
  readonly pageData = signal<ShipmentPageData | null>(null);
  readonly loading = signal(false);
  readonly loadingReady = signal(false);
  readonly openingMode = signal<'create' | 'request' | null>(null);
  readonly busyShipmentId = signal('');
  readonly inTransitShipmentId = signal('');
  readonly historyShipmentId = signal('');
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) {
        this.pageData.set(null);
        this.openingMode.set(null);
        this.shipments.set([]);
        this.readyShipments.set([]);
        this.load();
      }
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

  itemsTooltip(shipment: Shipment): string {
    return shipment.items.map((item) => `${item.productName} × ${item.quantity}`).join('\n');
  }

  get tableLoading(): boolean {
    return this.loading() || (this.mode === 'ready' && this.loadingReady());
  }

  get moduleTitle(): string {
    if (this.mode === 'pending') return 'Oczekujące na akceptację';
    if (this.mode === 'ready') return 'Pakowanie i wysyłka';
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
    this.runStep(row, this.service.approve(row.id), 'Wysyłka została zaakceptowana.');
  }

  pack(row: Shipment): void {
    if (!this.canApprove || row.status !== 'Zaakceptowana') return;
    this.runStep(
      row,
      this.service.pack(row.id),
      'Wysyłka została spakowana. Możesz wydrukować WZ.',
    );
  }

  dispatch(row: Shipment): void {
    if (!this.canApprove || row.status !== 'Spakowana') return;
    this.runStep(
      row,
      this.service.dispatch(row.id),
      'Wysyłka została wysłana, towar zdjęty ze stanu.',
    );
  }

  canCancel(row: Shipment): boolean {
    return ['Oczekuje na akceptację', 'Zaakceptowana', 'Spakowana'].includes(row.status);
  }

  hasDocuments(row: Shipment): boolean {
    return ['Spakowana', 'Wysłana', 'W drodze', 'Odebrana'].includes(row.status);
  }

  private runStep(row: Shipment, request: Observable<void>, successMessage: string): void {
    this.busyShipmentId.set(row.id);
    request.subscribe({
      next: () => {
        this.busyShipmentId.set('');
        this.notification.success(successMessage);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.busyShipmentId.set('');
        this.showError(error);
      },
    });
  }

  cancel(row: Shipment): void {
    if (!this.canApprove || !this.canCancel(row)) return;
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        width: '480px',
        data: {
          title: 'Anulowanie wysyłki',
          message: `Anulować wysyłkę ${row.number}? Rezerwacja towaru zostanie zwolniona.`,
          confirmLabel: 'Anuluj wysyłkę',
          cancelLabel: 'Zostaw',
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.runStep(row, this.service.cancel(row.id), 'Wysyłka została anulowana.');
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

  isActiveSource(row: Shipment): boolean {
    const activeWarehouseId = this.account.activeWarehouseId();
    return !activeWarehouseId || row.sourceWarehouseId === activeWarehouseId;
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
      if (type === 'label') {
        openPrintWindow(`Naklejka ${row.number}`, this.labelDocument(row, codes));
        return;
      }
      printDocument({
        title: `WZ ${row.number}`,
        heading: `WZ ${row.number}`,
        meta: [
          ['Miejsce wystawienia', row.sourceWarehouseName],
          ['Data wystawienia', formatPrintDate(row.createdOnUtc)],
          [
            'Data wydania towaru',
            formatPrintDate(row.sentOnUtc ?? row.packedOnUtc ?? row.approvedOnUtc),
          ],
          ['Status', row.status],
        ],
        parties: [
          {
            title: 'Wydający',
            lines: [
              row.sourceWarehouseName,
              row.sourceWarehouseAddress,
              taxLine(row.sourceWarehouseTaxNumber),
            ],
          },
          {
            title: 'Odbierający',
            lines: [
              row.destinationWarehouseName,
              row.destinationWarehouseAddress,
              taxLine(row.destinationWarehouseTaxNumber),
            ],
          },
        ],
        columns: [
          { label: 'Lp.', width: '36px', align: 'center' },
          { label: 'Nazwa towaru' },
          { label: 'SKU' },
          { label: 'Kod' },
          { label: 'Ilość', align: 'right', width: '70px' },
        ],
        rows: row.items.map((item, index) => [
          index + 1,
          item.productName,
          item.sku,
          item.barcode,
          item.quantity,
        ]),
        codes: { ...codes, identifier: row.id },
        signatures: [
          {
            title: 'Wystawił(a):',
            caption: 'Podpis osoby upoważnionej do wystawienia',
            signer: row.sentBy ?? row.packedBy ?? row.approvedBy ?? row.createdBy,
            signedOn: row.sentOnUtc ?? row.packedOnUtc ?? row.approvedOnUtc ?? row.createdOnUtc,
          },
          {
            title: 'Odebrał(a):',
            caption: 'Podpis osoby upoważnionej do odbioru',
            signer: row.receivedBy,
            signedOn: row.receivedOnUtc,
          },
        ],
      });
    } catch {
      this.notification.error('Nie udało się wygenerować kodów do druku.');
    }
  }

  private labelParty(name: string, address: string | null): string {
    return `<strong>${escapeHtml(name)}</strong>${address ? `<br />${escapeHtml(address)}` : ''}`;
  }

  private labelDocument(row: Shipment, codes: ShipmentPrintCodes): string {
    return `
      <style>
        .label { width: 420px; padding: 20px; border: 2px solid #111; }
        .label h2 { margin: 0 0 4px; font-size: 22px; }
        .label .muted { margin: 0 0 12px; color: #555; }
        .label .label-code { margin: 16px 0; text-align: center; }
        .label .label-code img { display: block; margin: 0 auto 10px; }
        .label .barcode { width: 260px; max-width: 100%; }
        .label .identifier { font-size: 11px; overflow-wrap: anywhere; }
        .label .party-title { margin-top: 10px; }
      </style>
      <div class="label">
        <h2>${escapeHtml(row.number)}</h2>
        <p class="muted">Wysyłka magazynowa</p>
        <div class="label-code">
          <img src="${codes.qr}" width="180" height="180" alt="QR wysyłki" />
          <img class="barcode" src="${codes.barcode}" alt="Kod kreskowy wysyłki" />
          <div class="identifier">${escapeHtml(row.id)}</div>
        </div>
        <div class="party-title">Skąd</div>
        <div class="party-body">${this.labelParty(row.sourceWarehouseName, row.sourceWarehouseAddress)}</div>
        <div class="party-title">Dokąd</div>
        <div class="party-body">${this.labelParty(row.destinationWarehouseName, row.destinationWarehouseAddress)}</div>
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
}
