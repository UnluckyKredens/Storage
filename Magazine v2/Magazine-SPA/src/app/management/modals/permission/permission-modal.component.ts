import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Permission } from '../../models/management.models';
import { PermissionService } from '../../services/permission.service';

interface PermissionModalData {
  mode: 'details' | 'form' | 'delete';
  row: Permission | null;
}

@Component({
  selector: 'app-permission-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormTextareaComponent,
  ],
  templateUrl: './permission-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class PermissionModalComponent {
  readonly data = inject<PermissionModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(PermissionService);
  private readonly dialogRef = inject(MatDialogRef<PermissionModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    code: String(this.data.row?.code ?? ''),
    name: String(this.data.row?.name ?? ''),
    description: String(this.data.row?.description ?? ''),
  };
  saving = false;

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły uprawnienia';
    if (this.data.mode === 'delete') return 'Usuń uprawnienie';
    return this.data.row ? 'Edytuj uprawnienie' : 'Dodaj uprawnienie';
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
