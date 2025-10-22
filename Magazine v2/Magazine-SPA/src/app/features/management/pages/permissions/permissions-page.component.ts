import { Component, inject } from '@angular/core';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { PermissionModalComponent } from '../../modals/permission/permission-modal.component';
import { Permission } from '../../models/management.models';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-permissions-page',
  imports: entityListPageImports,
  templateUrl: './permissions-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class PermissionsPageComponent extends EntityListPage<Permission> {
  protected readonly service = inject(PermissionService);
  protected readonly config = {
    managePermission: 'roles.manage',
    modal: PermissionModalComponent,
    added: 'Uprawnienie dodane.',
    updated: 'Uprawnienie zostało zaktualizowane.',
    removed: 'Uprawnienie usunięte.',
    loadError: 'Nie udało się pobrać uprawnień.',
  };
  readonly displayedColumns = ['code', 'name', 'description', 'actions'];
}
