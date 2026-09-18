import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { Shipment } from '../../models/management.models';
import { ShipmentService } from '../../services/shipment.service';

interface ShipmentReceiveModalData {
  identifier?: string;
}

@Component({
  selector: 'app-shipment-receive-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    MatIconModule,
    MatProgressBarModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormTextareaComponent,
  ],
  templateUrl: './shipment-receive-modal.component.html',
  styleUrls: ['../management-modal.scss', './shipment-receive-modal.component.scss'],
})
export class ShipmentReceiveModalComponent implements OnInit {
  readonly data = inject<ShipmentReceiveModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(ShipmentService);
  private readonly dialogRef = inject(MatDialogRef<ShipmentReceiveModalComponent, boolean>);

  identifier = '';
  notes = '';
  shipment: Shipment | null = null;
  checkedItemIds = new Set<string>();
  loading = signal(false);
  saving = signal(false);
  error = '';

  ngOnInit(): void {
    this.identifier = this.data.identifier ?? '';
    if (this.identifier) this.findShipment();
  }

  get allChecked(): boolean {
    return !!this.shipment?.items.length && this.shipment.items.every((item) => this.checkedItemIds.has(item.id));
  }

  get canSave(): boolean {
    return this.allChecked && this.shipment?.status === 'W drodze' && !this.loading() && !this.saving();
  }

  findShipment(): void {
    const identifier = this.identifier.trim();
    if (!identifier) {
      this.error = 'Zeskanuj albo wpisz identyfikator wysyłki.';
      return;
    }

    this.loading.set(true);
    this.error = '';
    this.shipment = null;
    this.checkedItemIds.clear();

    this.service.lookup(identifier).subscribe({
      next: (shipment) => {
        this.loading.set(false);
        this.shipment = shipment;
        if (shipment.status !== 'W drodze') {
          this.error = `Ta wysyłka ma status "${shipment.status}" i nie może zostać przyjęta.`;
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.error = error.error?.message ?? 'Nie znaleziono wysyłki.';
      },
    });
  }

  toggleItem(itemId: string, checked: boolean): void {
    if (checked) {
      this.checkedItemIds.add(itemId);
      return;
    }

    this.checkedItemIds.delete(itemId);
  }

  save(): void {
    if (!this.shipment) {
      this.error = 'Najpierw zeskanuj wysyłkę.';
      return;
    }
    if (!this.allChecked) {
      this.error = 'Potwierdź wszystkie pozycje z checklisty.';
      return;
    }

    this.saving.set(true);
    this.error = '';
    this.service
      .receive(this.shipment.id, {
        checkedItemIds: [...this.checkedItemIds],
        notes: this.notes.trim() || null,
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error = error.error?.message ?? 'Nie udało się przyjąć wysyłki.';
        },
      });
  }
}
