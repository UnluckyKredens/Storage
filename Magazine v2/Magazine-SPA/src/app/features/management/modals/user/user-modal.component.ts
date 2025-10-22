import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { administratorRoleId } from '../../../../core/auth/roles';
import { DialogHeaderComponent } from '../../../../shared/components/dialog-header/dialog-header.component';
import {
  CloseDialogActionsComponent,
  DeleteDialogBodyComponent,
  FormDialogActionsComponent,
} from '../../../../shared/components/dialog-parts/dialog-parts.component';
import { EntityDialog } from '../../../../core/entity/entity-dialog';
import { FormInputComponent } from '../../../../shared/components/form-input/form-input.component';
import { FormSelectComponent } from '../../../../shared/components/form-select/form-select.component';
import { User } from '../../models/management.models';
import { SelectOption } from '../../../../shared/models/master-data.models';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-user-modal',
  imports: [
    FormsModule,
    MatDialogModule,
    DialogHeaderComponent,
    CloseDialogActionsComponent,
    DeleteDialogBodyComponent,
    FormDialogActionsComponent,
    FormInputComponent,
    FormSelectComponent,
  ],
  templateUrl: './user-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class UserModalComponent extends EntityDialog<User> implements OnInit {
  private readonly service = inject(UserService);

  form = {
    login: String(this.data.row?.login ?? ''),
    firstName: String(this.data.row?.firstName ?? ''),
    lastName: String(this.data.row?.lastName ?? ''),
    email: String(this.data.row?.email ?? ''),
    password: '',
    roleId: String(this.data.row?.roleId ?? ''),
    warehouseId: this.data.row?.warehouseId ?? null,
  };
  roles: SelectOption[] = [];
  warehouses: SelectOption[] = [];

  protected readonly titles = {
    details: 'Szczegóły użytkownika',
    delete: 'Usuń użytkownika',
    edit: 'Edytuj użytkownika',
    add: 'Dodaj użytkownika',
  };

  protected request() {
    return this.service.save(this.data.row?.id ?? null, this.form);
  }

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

  get warehouseRequired(): boolean {
    return this.form.roleId !== administratorRoleId;
  }

  roleChanged(): void {
    if (!this.warehouseRequired) this.form.warehouseId = null;
  }
}
