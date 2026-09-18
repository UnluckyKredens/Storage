import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { StockMovement } from '../models/management.models';

export interface StockMovementFilters {
  productId?: string | null;
  warehouseId?: string | null;
  sourceId?: string | null;
  limit?: number;
}

@Injectable({ providedIn: 'root' })
export class StockMovementService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/StockMovements`;

  getAll(filters: StockMovementFilters = {}): Observable<StockMovement[]> {
    let params = new HttpParams();
    if (filters.productId) params = params.set('productId', filters.productId);
    if (filters.warehouseId) params = params.set('warehouseId', filters.warehouseId);
    if (filters.sourceId) params = params.set('sourceId', filters.sourceId);
    if (filters.limit) params = params.set('limit', filters.limit);

    return this.http.get<StockMovement[]>(this.apiUrl, { params });
  }
}
