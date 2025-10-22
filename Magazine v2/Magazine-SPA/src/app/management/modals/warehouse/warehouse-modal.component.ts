import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Warehouse } from '../../models/management.models';
import { WarehouseService } from '../../services/warehouse.service';

interface WarehouseModalData {
  mode: 'details' | 'form' | 'delete';
  row: Warehouse | null;
}

@Component({
  selector: 'app-warehouse-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormTextareaComponent,
  ],
  templateUrl: './warehouse-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class WarehouseModalComponent {
  readonly data = inject<WarehouseModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(WarehouseService);
  private readonly dialogRef = inject(MatDialogRef<WarehouseModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    name: String(this.data.row?.name ?? ''),
    address: String(this.data.row?.address ?? ''),
    description: String(this.data.row?.description ?? ''),
  };
  saving = false;

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły magazynu';
    if (this.data.mode === 'delete') return 'Usuń magazyn';
    return this.data.row ? 'Edytuj magazyn' : 'Dodaj magazyn';
  }

  save(): void {
    this.saving = true;

    this.service.save(this.data.row?.id ?? null, this.form).subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: HttpErrorResponse) => {
        this.saving = false;
        this.notification.error(error.error?.message ?? 'Nie udało się zapisać danych.');
      },
    });
  }

  display(value: unknown): string {
    if (value === null || value === undefined || value === '') return '—';
    if (typeof value === 'boolean') return value ? 'Tak' : 'Nie';
    if (Array.isArray(value)) return value.join(', ') || '—';
    return String(value);
  }
}
