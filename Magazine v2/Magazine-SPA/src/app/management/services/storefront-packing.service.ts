import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StorefrontOrderHistory, StorefrontPackingOrder, StorefrontPoolOrder } from '../models/management.models';

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

  getHistory(status: number | null, days: number): Observable<StorefrontOrderHistory[]> {
    const params: Record<string, number> = { days };
    if (status !== null) params['status'] = status;
    return this.http.get<StorefrontOrderHistory[]>(`${this.apiUrl}/history`, { params });
  }

  ship(id: string, carrierName: string, trackingNumber: string | null): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/ship`, { carrierName, trackingNumber });
  }

  getPool(): Observable<StorefrontPoolOrder[]> {
    return this.http.get<StorefrontPoolOrder[]>(`${this.apiUrl}/pool`);
  }

  claim(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/claim`, {});
  }
}
