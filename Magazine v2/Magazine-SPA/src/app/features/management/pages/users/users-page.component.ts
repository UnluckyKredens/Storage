import { Component, inject } from '@angular/core';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { UserModalComponent } from '../../modals/user/user-modal.component';
import { User } from '../../models/management.models';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-users-page',
  imports: entityListPageImports,
  templateUrl: './users-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class UsersPageComponent extends EntityListPage<User> {
  protected readonly service = inject(UserService);
  protected readonly config = {
    managePermission: 'users.manage',
    modal: UserModalComponent,
    added: 'Użytkownik dodany.',
    updated: 'Użytkownik został zaktualizowany.',
    removed: 'Użytkownik usunięty.',
    loadError: 'Nie udało się pobrać użytkowników.',
  };
  protected override readonly reloadOnWarehouseChange = true;
  readonly displayedColumns = [
    'login',
    'firstName',
    'lastName',
    'email',
    'roleName',
    'warehouseName',
    'actions',
  ];
}
