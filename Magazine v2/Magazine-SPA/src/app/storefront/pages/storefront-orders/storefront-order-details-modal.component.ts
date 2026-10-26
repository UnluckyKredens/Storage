import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { StorefrontOrderHistory } from '../../models/storefront.models';
import { printSalesDocument } from './print-sales-document';

@Component({
  selector: 'app-storefront-order-details-modal',
  imports: [DatePipe, DecimalPipe, MatButtonModule, MatDialogModule, DialogHeaderComponent],
  template: `
    <app-dialog-header [title]="'Zamówienie ' + data.order.number" />
    <mat-dialog-content>
      <dl>
        <dt>Status</dt>
        <dd>{{ data.order.status }}</dd>
        <dt>Klient</dt>
        <dd>{{ data.order.customerName }}</dd>
        <dt>E-mail / telefon</dt>
        <dd>{{ data.order.customerEmail }} {{ data.order.customerPhone ? ' / ' + data.order.customerPhone : '' }}</dd>
        <dt>Adres dostawy</dt>
        <dd>{{ data.order.deliveryAddress || '—' }}</dd>
        <dt>Uwagi klienta</dt>
        <dd>{{ data.order.notes || '—' }}</dd>
        <dt>Zamówiono</dt>
        <dd>{{ data.order.createdOnUtc | date: 'dd.MM.yyyy HH:mm' }}</dd>
        <dt>Przyjął do realizacji</dt>
        <dd>{{ data.order.acceptedBy || '—' }} {{ data.order.acceptedOnUtc | date: 'dd.MM.yyyy HH:mm' }}</dd>
        <dt>Spakował</dt>
        <dd>{{ data.order.packedBy || '—' }} {{ data.order.packedOnUtc | date: 'dd.MM.yyyy HH:mm' }}</dd>
        <dt>Przekazał przewoźnikowi</dt>
        <dd>{{ data.order.shippedBy || '—' }} {{ data.order.shippedOnUtc | date: 'dd.MM.yyyy HH:mm' }}</dd>
        <dt>Przewoźnik / przesyłka</dt>
        <dd>{{ data.order.carrierName || '—' }} {{ data.order.carrierTrackingNumber ? ' / ' + data.order.carrierTrackingNumber : '' }}</dd>
      </dl>
      <table>
        <thead>
          <tr><th>Produkt</th><th>SKU</th><th>Ilość</th><th>Cena</th><th>Wartość</th></tr>
        </thead>
        <tbody>
          @for (item of data.order.items; track item.sku) {
            <tr>
              <td>{{ item.productName }}</td>
              <td>{{ item.sku }}</td>
              <td>{{ item.quantity | number: '1.0-3' }}</td>
              <td>{{ item.unitPrice | number: '1.2-2' }}</td>
              <td>{{ item.totalPrice | number: '1.2-2' }}</td>
            </tr>
          }
        </tbody>
      </table>
      <p class="total">Razem: <strong>{{ data.order.totalValue | number: '1.2-2' }} zł</strong></p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="print()">Dokument</button>
      <button mat-flat-button type="button" mat-dialog-close>Zamknij</button>
    </mat-dialog-actions>
  `,
  styles: `
    dl {
      display: grid;
      grid-template-columns: 190px 1fr;
      gap: 8px 16px;
      margin: 0 0 16px;
    }
    dt {
      color: var(--app-muted);
      font-size: 13px;
      font-weight: 700;
    }
    dd {
      margin: 0;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th,
    td {
      padding: 8px 6px;
      border-top: 1px solid var(--app-border);
      text-align: left;
    }
    .total {
      text-align: right;
    }
  `,
})
export class StorefrontOrderDetailsModalComponent {
  readonly data = inject<{ order: StorefrontOrderHistory }>(MAT_DIALOG_DATA);

  print(): void {
    printSalesDocument(this.data.order);
  }
}
