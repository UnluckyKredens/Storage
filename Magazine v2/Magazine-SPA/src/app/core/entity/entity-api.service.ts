import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export abstract class EntityApiService<T, F> {
  protected readonly http = inject(HttpClient);
  protected readonly apiUrl: string;

  protected constructor(resource: string) {
    this.apiUrl = `${environment.apiUrl}/${resource}`;
  }

  getAll(): Observable<T[]> {
    return this.http.get<T[]>(this.apiUrl);
  }

  getById(id: string): Observable<T> {
    return this.http.get<T>(`${this.apiUrl}/${id}`);
  }

  save(id: string | null, form: F): Observable<T> {
    return id
      ? this.http.put<T>(`${this.apiUrl}/${id}`, form)
      : this.http.post<T>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
