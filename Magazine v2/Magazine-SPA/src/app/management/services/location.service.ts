import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Location, SelectOption, Warehouse } from '../models/management.models';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Locations`;

  getAll(): Observable<Location[]> {
    return this.http.get<Location[]>(this.apiUrl);
  }

  getById(id: string): Observable<Location> {
    return this.http.get<Location>(`${this.apiUrl}/${id}`);
  }

  save(
    id: string | null,
    form: { warehouseId: string; locationCode: string; description: string },
  ): Observable<Location> {
    return id
      ? this.http.put<Location>(`${this.apiUrl}/${id}`, form)
      : this.http.post<Location>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getFormOptions(): Observable<SelectOption[]> {
    return this.http
      .get<Warehouse[]>(`${this.apiUrl}/page-data`)
      .pipe(map((items) => items.map((item) => ({ value: item.id, label: item.name }))));
  }
}
