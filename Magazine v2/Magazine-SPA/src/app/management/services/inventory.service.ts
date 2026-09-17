import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InventoryItem, SelectOption } from '../models/management.models';

export interface InventoryForm {
  productId: string;
  locationId: string;
  quantity: number;
  reservedQuantity: number;
}

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Inventory`;

  getAll(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(this.apiUrl);
  }

  getById(id: string): Observable<InventoryItem> {
    return this.http.get<InventoryItem>(`${this.apiUrl}/${id}`);
  }

  save(id: string | null, form: InventoryForm): Observable<InventoryItem> {
    return id
      ? this.http.put<InventoryItem>(`${this.apiUrl}/${id}`, form)
      : this.http.post<InventoryItem>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getFormOptions(): Observable<{ products: SelectOption[]; locations: SelectOption[] }> {
    return this.http
      .get<{
        products: { id: string; name: string }[];
        locations: { id: string; code: string; warehouseName: string }[];
      }>(`${this.apiUrl}/page-data`)
      .pipe(
        map((data) => ({
          products: data.products.map((item) => ({ value: item.id, label: item.name })),
          locations: data.locations.map((item) => ({
            value: item.id,
            label: `${item.warehouseName} / ${item.code}`,
          })),
        })),
      );
  }
}
