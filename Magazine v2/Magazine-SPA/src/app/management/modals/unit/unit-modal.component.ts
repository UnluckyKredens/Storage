import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { UnitOfMeasure } from '../../models/management.models';
import { UnitService } from '../../services/unit.service';

interface UnitModalData {
  mode: 'details' | 'form' | 'delete';
  row: UnitOfMeasure | null;
}

@Component({
  selector: 'app-unit-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
  ],
  templateUrl: './unit-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class UnitModalComponent {
  readonly data = inject<UnitModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(UnitService);
  private readonly dialogRef = inject(MatDialogRef<UnitModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    name: String(this.data.row?.name ?? ''),
    symbol: String(this.data.row?.symbol ?? ''),
  };
  saving = false;

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły jednostki miary';
    if (this.data.mode === 'delete') return 'Usuń jednostkę miary';
    return this.data.row ? 'Edytuj jednostkę miary' : 'Dodaj jednostkę miary';
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
