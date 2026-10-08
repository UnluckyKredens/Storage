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
import { User, SelectOption } from '../../models/management.models';
import { UserService } from '../../services/user.service';

interface UserModalData {
  mode: 'details' | 'form' | 'delete';
  row: User | null;
}

@Component({
  selector: 'app-user-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormSelectComponent,
  ],
  templateUrl: './user-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class UserModalComponent implements OnInit {
  readonly data = inject<UserModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(UserService);
  private readonly dialogRef = inject(MatDialogRef<UserModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    login: String(this.data.row?.login ?? ''),
    firstName: String(this.data.row?.firstName ?? ''),
    lastName: String(this.data.row?.lastName ?? ''),
    email: String(this.data.row?.email ?? ''),
    password: '',
    roleId: String(this.data.row?.roleId ?? ''),
    warehouseId: String(this.data.row?.warehouseId ?? ''),
  };
  saving = false;
  roles: SelectOption[] = [];
  warehouses: SelectOption[] = [];

  ngOnInit(): void {
    if (this.data.mode !== 'form') return;
    this.service.getFormOptions().subscribe({
      next: (options) => {
        this.roles = options.roles;
        this.warehouses = options.warehouses;
      },
      error: () => this.notification.error('Nie udało się pobrać ról i magazynów.'),
    });
  }

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły użytkownika';
    if (this.data.mode === 'delete') return 'Usuń użytkownika';
    return this.data.row ? 'Edytuj użytkownika' : 'Dodaj użytkownika';
  }

  get warehouseRequired(): boolean {
    return this.form.roleId !== administratorRoleId;
  }

  roleChanged(): void {
    if (!this.warehouseRequired) this.form.warehouseId = '';
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
