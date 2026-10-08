import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Permission } from '../models/management.models';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Permissions`;

  getAll(): Observable<Permission[]> {
    return this.http.get<Permission[]>(this.apiUrl);
  }

  getById(id: string): Observable<Permission> {
    return this.http.get<Permission>(`${this.apiUrl}/${id}`);
  }

  save(
    id: string | null,
    form: { code: string; name: string; description: string },
  ): Observable<Permission> {
    return id
      ? this.http.put<Permission>(`${this.apiUrl}/${id}`, form)
      : this.http.post<Permission>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
