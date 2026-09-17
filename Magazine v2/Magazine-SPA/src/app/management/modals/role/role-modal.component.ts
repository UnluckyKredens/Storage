import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { administratorRoleId } from '../../../auth/roles';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormSelectComponent } from '../../../shared/components/form-select/form-select.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Role, SelectOption } from '../../models/management.models';
import { RoleService } from '../../services/role.service';

interface RoleModalData {
  mode: 'details' | 'form' | 'delete';
  row: Role | null;
}

@Component({
  selector: 'app-role-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormSelectComponent,
  ],
  templateUrl: './role-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class RoleModalComponent implements OnInit {
  readonly data = inject<RoleModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(RoleService);
  private readonly dialogRef = inject(MatDialogRef<RoleModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    name: String(this.data.row?.name ?? ''),
    permissionCodes: Array.isArray(this.data.row?.permissionCodes)
      ? this.data.row.permissionCodes.map(String)
      : [],
  };
  saving = false;
  permissions: SelectOption[] = [];

  ngOnInit(): void {
    if (this.data.mode !== 'form') return;
    this.service.getFormOptions().subscribe({
      next: (permissions) => (this.permissions = permissions),
      error: () => this.notification.error('Nie udało się pobrać uprawnień.'),
    });
  }

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły roli';
    if (this.data.mode === 'delete') return 'Usuń rolę';
    return this.data.row ? 'Edytuj rolę' : 'Dodaj rolę';
  }

  get nameDisabled(): boolean {
    return !!this.data.row && this.data.row.id === administratorRoleId;
  }

  get permissionsDisabled(): boolean {
    return this.data.row?.id === administratorRoleId;
  }

  save(): void {
    this.saving = true;
    this.service.save(this.data.row, this.form).subscribe({
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
