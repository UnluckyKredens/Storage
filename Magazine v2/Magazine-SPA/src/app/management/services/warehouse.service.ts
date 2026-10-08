import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Warehouse } from '../models/management.models';

@Injectable({ providedIn: 'root' })
export class WarehouseService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Warehouses`;

  getAll(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(this.apiUrl);
  }

  getById(id: string): Observable<Warehouse> {
    return this.http.get<Warehouse>(`${this.apiUrl}/${id}`);
  }

  save(
    id: string | null,
    form: { name: string; address: string; description: string },
  ): Observable<Warehouse> {
    return id
      ? this.http.put<Warehouse>(`${this.apiUrl}/${id}`, form)
      : this.http.post<Warehouse>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
