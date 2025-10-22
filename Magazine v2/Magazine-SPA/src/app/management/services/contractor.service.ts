import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Contractor } from '../models/management.models';

export interface ContractorForm {
  name: string;
  taxNumber: string;
  type: number;
  email: string;
  phone: string;
  address: string;
}

@Injectable({ providedIn: 'root' })
export class ContractorService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Contractors`;

  getAll(): Observable<Contractor[]> {
    return this.http.get<Contractor[]>(this.apiUrl);
  }

  getById(id: string): Observable<Contractor> {
    return this.http.get<Contractor>(`${this.apiUrl}/${id}`);
  }

  save(id: string | null, form: ContractorForm): Observable<Contractor> {
    return id
      ? this.http.put<Contractor>(`${this.apiUrl}/${id}`, form)
      : this.http.post<Contractor>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
