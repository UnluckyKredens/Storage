import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import {
  FormSelectComponent,
  FormSelectOption,
} from '../../../shared/components/form-select/form-select.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Contractor } from '../../models/management.models';
import { ContractorService } from '../../services/contractor.service';

interface ContractorModalData {
  mode: 'details' | 'form' | 'delete';
  row: Contractor | null;
}

@Component({
  selector: 'app-contractor-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormSelectComponent,
    FormTextareaComponent,
  ],
  templateUrl: './contractor-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class ContractorModalComponent {
  readonly data = inject<ContractorModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(ContractorService);
  private readonly dialogRef = inject(MatDialogRef<ContractorModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    name: String(this.data.row?.name ?? ''),
    taxNumber: String(this.data.row?.taxNumber ?? ''),
    type: Number(this.data.row?.type ?? 1),
    email: String(this.data.row?.email ?? ''),
    phone: String(this.data.row?.phone ?? ''),
    address: String(this.data.row?.address ?? ''),
  };
  saving = false;
  contractorTypes: FormSelectOption[] = [
    { value: 1, label: 'Dostawca' },
    { value: 2, label: 'Odbiorca' },
    { value: 3, label: 'Oba' },
  ];

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły kontrahenta';
    if (this.data.mode === 'delete') return 'Usuń kontrahenta';
    return this.data.row ? 'Edytuj kontrahenta' : 'Dodaj kontrahenta';
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

  contractorType(): string {
    const type = Number(this.data.row?.type);
    if (type === 1) return 'Dostawca';
    if (type === 2) return 'Odbiorca';
    if (type === 3) return 'Oba';
    return '—';
  }
}
