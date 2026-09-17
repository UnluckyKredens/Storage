import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Category,
  Product,
  ProductPage,
  SelectOption,
  UnitOfMeasure,
} from '../models/management.models';

export interface ProductForm {
  name: string;
  sku: string;
  barcode: string;
  categoryId: string;
  unitOfMeasureId: string;
  purchasePrice: number;
  salePrice: number;
  description: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Products`;

  getAll(
    page: number,
    pageSize: number,
    search: string,
    sortBy: string,
    order: 'asc' | 'desc',
  ): Observable<ProductPage> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('search', search)
      .set('sortBy', sortBy)
      .set('order', order);

    return this.http.get<ProductPage>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  save(id: string | null, form: ProductForm): Observable<Product> {
    return id
      ? this.http.put<Product>(`${this.apiUrl}/${id}`, form)
      : this.http.post<Product>(this.apiUrl, form);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getFormOptions(): Observable<{ categories: SelectOption[]; units: SelectOption[] }> {
    return this.http
      .get<{ categories: Category[]; units: UnitOfMeasure[] }>(`${this.apiUrl}/page-data`)
      .pipe(
        map((data) => ({
          categories: data.categories.map((item) => ({ value: item.id, label: item.name })),
          units: data.units.map((item) => ({ value: item.id, label: item.name })),
        })),
      );
  }
}
