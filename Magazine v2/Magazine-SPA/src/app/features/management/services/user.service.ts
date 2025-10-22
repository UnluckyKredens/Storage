import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { administratorRoleId } from '../../../core/auth/roles';
import { EntityApiService } from '../../../core/entity/entity-api.service';
import { Role, User } from '../models/management.models';
import { SelectOption, Warehouse } from '../../../shared/models/master-data.models';

export interface UserForm {
  login: string;
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  roleId: string;
  warehouseId: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserService extends EntityApiService<User, UserForm> {
  constructor() {
    super('Users');
  }

  override save(id: string | null, form: UserForm): Observable<User> {
    const warehouseId = form.roleId === administratorRoleId ? null : form.warehouseId || null;
    const keepPassword = !id || !!form.password;
    return super.save(id, {
      ...form,
      warehouseId,
      password: keepPassword ? form.password : undefined,
    } as UserForm);
  }

  getFormOptions(): Observable<{ roles: SelectOption[]; warehouses: SelectOption[] }> {
    return this.http
      .get<{ roles: Role[]; warehouses: Warehouse[] }>(`${this.apiUrl}/page-data`)
      .pipe(
        map((data) => ({
          roles: data.roles.map((item) => ({ value: item.id, label: item.name })),
          warehouses: data.warehouses.map((item) => ({ value: item.id, label: item.name })),
        })),
      );
  }
}
