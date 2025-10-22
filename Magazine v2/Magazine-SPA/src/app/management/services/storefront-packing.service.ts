import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StorefrontPackingOrder } from '../models/management.models';

@Injectable({ providedIn: 'root' })
export class StorefrontPackingService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Storefront/orders`;

  getPackingOrders(): Observable<StorefrontPackingOrder[]> {
    return this.http.get<StorefrontPackingOrder[]>(`${this.apiUrl}/packing`);
  }

  accept(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/accept`, {});
  }

  pack(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/pack`, {});
  }
}
