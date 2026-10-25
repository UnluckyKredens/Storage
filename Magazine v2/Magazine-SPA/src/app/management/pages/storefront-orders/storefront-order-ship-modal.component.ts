import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { StorefrontOrderHistory } from '../../models/management.models';
import { StorefrontPackingService } from '../../services/storefront-packing.service';

@Component({
  selector: 'app-storefront-order-ship-modal',
  imports: [FormsModule, MatButtonModule, MatDialogModule, DialogHeaderComponent],
  template: `
    <app-dialog-header title="Przekaż kurierowi" />
    <form #shipForm="ngForm" (ngSubmit)="save()">
      <mat-dialog-content>
        <p class="hint">
          Zamówienie {{ data.order.number }} dla {{ data.order.customerName }}. Po zapisaniu zmieni status na
          „Wysłane”, a klient zobaczy przewoźnika w śledzeniu.
        </p>
        <label class="field">
          <span>Przewoźnik</span>
          <input
            name="carrierName"
            [(ngModel)]="carrierName"
            list="carriers"
            maxlength="100"
            placeholder="np. InPost, DPD, DHL"
            required
          />
          <datalist id="carriers">
            @for (carrier of carriers; track carrier) {
              <option [value]="carrier"></option>
            }
          </datalist>
        </label>
        <label class="field">
          <span>Numer przesyłki (opcjonalnie)</span>
          <input name="trackingNumber" [(ngModel)]="trackingNumber" maxlength="100" />
        </label>
        @if (error) {
          <p class="error">{{ error }}</p>
        }
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close>Anuluj</button>
        <button mat-flat-button type="submit" [disabled]="shipForm.invalid || saving()">
          {{ saving() ? 'Zapisywanie...' : 'Zapisz wysyłkę' }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    .hint {
      margin: 0 0 16px;
      color: var(--app-muted);
    }
    .field {
      display: grid;
      gap: 6px;
      margin-bottom: 14px;
      font-size: 13px;
      color: var(--app-muted);
    }
    input {
      height: 44px;
      padding: 0 12px;
      border: 1px solid var(--app-border);
      border-radius: 8px;
      background: var(--app-surface);
      color: var(--app-text);
      font: inherit;
    }
    .error {
      color: #b91c1c;
    }
  `,
})
export class StorefrontOrderShipModalComponent {
  readonly data = inject<{ order: StorefrontOrderHistory }>(MAT_DIALOG_DATA);
  private readonly service = inject(StorefrontPackingService);
  private readonly dialogRef = inject(MatDialogRef<StorefrontOrderShipModalComponent, boolean>);

  readonly carriers = ['InPost', 'DPD', 'DHL', 'UPS', 'FedEx', 'Poczta Polska', 'GLS'];
  readonly saving = signal(false);
  carrierName = '';
  trackingNumber = '';
  error = '';

  save(): void {
    if (!this.carrierName.trim() || this.saving()) return;
    this.saving.set(true);
    this.error = '';
    this.service.ship(this.data.order.id, this.carrierName, this.trackingNumber.trim() || null).subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.error = error.error?.message ?? 'Nie udało się zapisać wysyłki.';
      },
    });
  }
}
