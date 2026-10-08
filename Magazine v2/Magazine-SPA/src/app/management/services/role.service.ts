import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, of, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { administratorRoleId } from '../../auth/roles';
import { Role, SelectOption } from '../models/management.models';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Roles`;

  getAll(): Observable<Role[]> {
    return this.http.get<Role[]>(this.apiUrl);
  }

  getById(id: string): Observable<Role> {
    return this.http.get<Role>(`${this.apiUrl}/${id}`);
  }

  save(role: Role | null, form: { name: string; permissionCodes: string[] }): Observable<Role> {
    if (!role) {
      return this.http
        .post<Role>(this.apiUrl, { name: form.name })
        .pipe(
          switchMap((createdRole) => this.savePermissions(createdRole.id, form.permissionCodes)),
        );
    }

    if (role.id === administratorRoleId) return of(role);

    const saveName =
      role.id === administratorRoleId
        ? of(role)
        : this.http.put<Role>(`${this.apiUrl}/${role.id}`, { name: form.name });

    return saveName.pipe(switchMap(() => this.savePermissions(role.id, form.permissionCodes)));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getFormOptions(): Observable<SelectOption[]> {
    return this.http.get<{ code: string; name: string }[]>(`${this.apiUrl}/permissions`).pipe(
      map((items) =>
        items.map((item) => ({
          value: item.code,
          label: `${item.name} (${item.code})`,
        })),
      ),
    );
  }

  private savePermissions(id: string, permissionCodes: string[]): Observable<Role> {
    return this.http.put<Role>(`${this.apiUrl}/${id}/permissions`, { permissionCodes });
  }
}
