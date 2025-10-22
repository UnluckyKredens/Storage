import { Component, inject } from '@angular/core';
import { administratorRoleId, isSystemRole } from '../../../../core/auth/roles';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { RoleModalComponent } from '../../modals/role/role-modal.component';
import { Role } from '../../models/management.models';
import { RoleService } from '../../services/role.service';

@Component({
  selector: 'app-roles-page',
  imports: entityListPageImports,
  templateUrl: './roles-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class RolesPageComponent extends EntityListPage<Role> {
  protected override readonly service = inject(RoleService);
  protected readonly config = {
    managePermission: 'roles.manage',
    modal: RoleModalComponent,
    added: 'Rola dodana.',
    updated: 'Rola została zaktualizowana.',
    removed: 'Rola usunięta.',
    loadError: 'Nie udało się pobrać ról.',
  };
  readonly displayedColumns = ['name', 'permissionCodes', 'actions'];
  permissionNames: Record<string, string> = {};

  override ngOnInit(): void {
    this.service.getPermissionNames().subscribe((names) => (this.permissionNames = names));
    super.ngOnInit();
  }

  permissionLabels(row: Role): string {
    if (row.hasAllPermissions) return 'Wszystkie';
    return (
      row.permissionCodes.map((code) => this.permissionNames[code] ?? code).join(', ') || 'Brak'
    );
  }

  override canEdit(row: Role): boolean {
    return this.canManage && row.id !== administratorRoleId;
  }

  override canDelete(row: Role): boolean {
    return this.canManage && !isSystemRole(row.id);
  }
}
