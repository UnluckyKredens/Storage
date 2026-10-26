import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { SearchSelectComponent } from '../../../shared/components/search-select/search-select.component';
import { FormSelectComponent } from '../../../shared/components/form-select/form-select.component';
import { InventoryItem } from '../../models/inventory.models';
import { SelectOption } from '../../../management/models/management.models';
import { InventoryService } from '../../services/inventory.service';

interface InventoryModalData {
  mode: 'details' | 'form' | 'delete';
  row: InventoryItem | null;
}

@Component({
  selector: 'app-inventory-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormSelectComponent,
    SearchSelectComponent,
  ],
  templateUrl: './inventory-modal.component.html',
  styleUrl: '../../../shared/styles/management-modal.scss',
})
export class InventoryModalComponent implements OnInit {
  readonly data = inject<InventoryModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(InventoryService);
  private readonly dialogRef = inject(MatDialogRef<InventoryModalComponent, boolean>);

  form = {
    productId: String(this.data.row?.productId ?? ''),
    locationId: String(this.data.row?.locationId ?? ''),
    quantity: Number(this.data.row?.quantity ?? 0),
    reservedQuantity: Number(this.data.row?.reservedQuantity ?? 0),
  };
  saving = signal(false);
  error = '';
  readonly products = signal<SelectOption[]>(this.rowOptions().products);
  readonly locations = signal<SelectOption[]>(this.rowOptions().locations);

  ngOnInit(): void {
    if (this.data.mode !== 'form' || this.data.row) return;
    this.service.getFormOptions().subscribe({
      next: (options) => {
        this.products.set(options.products);
        this.locations.set(options.locations);
      },
      error: () => (this.error = 'Nie udało się pobrać produktów i lokalizacji.'),
    });
  }

  private rowOptions(): { products: SelectOption[]; locations: SelectOption[] } {
    const row = this.data.row;
    if (!row) return { products: [], locations: [] };
    return {
      products: [{ value: row.productId, label: row.productName }],
      locations: [{ value: row.locationId, label: `${row.warehouseName} / ${row.locationCode}` }],
    };
  }

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły stanu magazynowego';
    if (this.data.mode === 'delete') return 'Usuń stan magazynowy';
    return this.data.row ? 'Edytuj stan magazynowy' : 'Dodaj stan magazynowy';
  }

  save(): void {
    if (!this.formIsValid()) return;
    this.saving.set(true);
    this.error = '';

    this.service.save(this.data.row?.id ?? null, this.form).subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.error = error.error?.message ?? 'Nie udało się zapisać danych.';
      },
    });
  }

  display(value: unknown): string {
    if (value === null || value === undefined || value === '') return '—';
    if (typeof value === 'boolean') return value ? 'Tak' : 'Nie';
    if (Array.isArray(value)) return value.join(', ') || '—';
    return String(value);
  }

  private formIsValid(): boolean {
    if (this.form.reservedQuantity <= this.form.quantity) return true;
    this.error = 'Rezerwacja nie może przekraczać ilości.';
    return false;
  }
}
