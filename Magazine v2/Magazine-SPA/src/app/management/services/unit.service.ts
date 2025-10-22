import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UnitOfMeasure } from '../models/management.models';

@Injectable({ providedIn: 'root' })
export class UnitService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/UnitsOfMeasure`;

  getAll(): Observable<UnitOfMeasure[]> {
    return this.http.get<UnitOfMeasure[]>(this.apiUrl);
  }

  getById(id: string): Observable<UnitOfMeasure> {
    return this.http.get<UnitOfMeasure>(`${this.apiUrl}/${id}`);
  }

  save(id: string | null, form: { name: string; symbol: string }): Observable<UnitOfMeasure> {
    return id
      ? this.http.put<UnitOfMeasure>(`${this.apiUrl}/${id}`, form)
      : this.http.post<UnitOfMeasure>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
