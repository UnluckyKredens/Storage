import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { administratorRoleId, isSystemRole } from '../../../../core/auth/roles';
import { DialogHeaderComponent } from '../../../../shared/components/dialog-header/dialog-header.component';
import {
  CloseDialogActionsComponent,
  DeleteDialogBodyComponent,
  FormDialogActionsComponent,
} from '../../../../shared/components/dialog-parts/dialog-parts.component';
import { EntityDialog } from '../../../../core/entity/entity-dialog';
import { FormInputComponent } from '../../../../shared/components/form-input/form-input.component';
import { FormSelectComponent } from '../../../../shared/components/form-select/form-select.component';
import { Role } from '../../models/management.models';
import { SelectOption } from '../../../../shared/models/master-data.models';
import { RoleService } from '../../services/role.service';

@Component({
  selector: 'app-role-modal',
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
  templateUrl: './role-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class RoleModalComponent extends EntityDialog<Role> implements OnInit {
  private readonly service = inject(RoleService);

  form = {
    name: String(this.data.row?.name ?? ''),
    permissionCodes: Array.isArray(this.data.row?.permissionCodes)
      ? this.data.row.permissionCodes.map(String)
      : [],
  };
  permissions: SelectOption[] = [];

  permissionNames: Record<string, string> = {};

  get detailPermissions(): string {
    if (this.data.row?.hasAllPermissions) return 'Wszystkie';
    const codes: string[] = this.data.row?.permissionCodes ?? [];
    return codes.map((code) => this.permissionNames[code] ?? code).join(', ') || '—';
  }

  protected readonly titles = {
    details: 'Szczegóły roli',
    delete: 'Usuń rolę',
    edit: 'Edytuj rolę',
    add: 'Dodaj rolę',
  };

  protected request() {
    return this.service.save(this.data.row, this.form);
  }

  ngOnInit(): void {
    if (this.data.mode === 'details') {
      this.service.getPermissionNames().subscribe((names) => (this.permissionNames = names));
    }
    if (this.data.mode !== 'form') return;
    this.service.getFormOptions().subscribe({
      next: (permissions) => (this.permissions = permissions),
      error: () => this.notification.error('Nie udało się pobrać uprawnień.'),
    });
  }

  get nameDisabled(): boolean {
    return !!this.data.row && isSystemRole(this.data.row.id);
  }

  get permissionsDisabled(): boolean {
    return this.data.row?.id === administratorRoleId;
  }
}
