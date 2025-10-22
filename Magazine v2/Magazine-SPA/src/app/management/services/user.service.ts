import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { administratorRoleId } from '../../auth/roles';
import { Role, SelectOption, User, Warehouse } from '../models/management.models';

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
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Users`;

  getAll(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  getById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  save(id: string | null, form: UserForm): Observable<User> {
    const payload = { ...form, warehouseId: form.roleId === administratorRoleId ? null : form.warehouseId || null };
    if (!id) return this.http.post<User>(this.apiUrl, payload);

    const { password, ...formWithoutPassword } = payload;
    const body = password ? payload : formWithoutPassword;
    return this.http.put<User>(`${this.apiUrl}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
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
