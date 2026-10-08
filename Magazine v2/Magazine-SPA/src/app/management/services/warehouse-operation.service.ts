import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WarehouseOperation } from '../models/management.models';

export interface WarehouseOperationFormItem {
  productId: string;
  sourceLocationId: string | null;
  destinationLocationId: string | null;
  quantity: number;
  targetQuantity: number | null;
}

export interface WarehouseOperationForm {
  type: number;
  warehouseId: string | null;
  notes: string | null;
  items: WarehouseOperationFormItem[];
}

@Injectable({ providedIn: 'root' })
export class WarehouseOperationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/WarehouseOperations`;

  getAll(): Observable<WarehouseOperation[]> {
    return this.http.get<WarehouseOperation[]>(this.apiUrl);
  }

  complete(form: WarehouseOperationForm): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/complete`, form);
  }
}
