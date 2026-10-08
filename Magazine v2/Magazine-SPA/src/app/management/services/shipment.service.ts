import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Shipment,
  ShipmentHistory,
  ShipmentPageData,
  ShipmentProduct,
} from '../models/management.models';

export interface ShipmentFormItem {
  barcode?: string;
  productId?: string;
  quantity: number;
}

export interface ShipmentForm {
  destinationWarehouseId: string;
  items: ShipmentFormItem[];
}

export interface ShipmentRequestForm {
  items: ShipmentFormItem[];
}

export interface ShipmentReceiveForm {
  checkedItemIds: string[];
  notes: string | null;
}

@Injectable({ providedIn: 'root' })
export class ShipmentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Shipments`;

  getAll(): Observable<Shipment[]> {
    return this.http.get<Shipment[]>(this.apiUrl);
  }

  getReady(): Observable<Shipment[]> {
    return this.http.get<Shipment[]>(`${this.apiUrl}/ready`);
  }

  getPending(): Observable<Shipment[]> {
    return this.http.get<Shipment[]>(`${this.apiUrl}/pending`);
  }

  getById(id: string): Observable<Shipment> {
    return this.http.get<Shipment>(`${this.apiUrl}/${id}`);
  }

  lookup(identifier: string): Observable<Shipment> {
    const params = new HttpParams().set('identifier', identifier);
    return this.http.get<Shipment>(`${this.apiUrl}/lookup`, { params });
  }

  getHistory(id: string): Observable<ShipmentHistory[]> {
    return this.http.get<ShipmentHistory[]>(`${this.apiUrl}/${id}/history`);
  }

  getPageData(): Observable<ShipmentPageData> {
    return this.http.get<ShipmentPageData>(`${this.apiUrl}/page-data`);
  }

  getProductByBarcode(barcode: string): Observable<ShipmentProduct> {
    const params = new HttpParams().set('barcode', barcode);
    return this.http.get<ShipmentProduct>(`${this.apiUrl}/product-by-barcode`, { params });
  }

  getProduct(productId: string): Observable<ShipmentProduct> {
    const params = new HttpParams().set('productId', productId);
    return this.http.get<ShipmentProduct>(`${this.apiUrl}/product`, { params });
  }

  create(form: ShipmentForm): Observable<Shipment> {
    return this.http.post<Shipment>(this.apiUrl, form);
  }

  createRequest(form: ShipmentRequestForm): Observable<Shipment> {
    return this.http.post<Shipment>(`${this.apiUrl}/request`, form);
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/approve`, {});
  }

  markInTransit(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/in-transit`, {});
  }

  receive(id: string, form: ShipmentReceiveForm): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/receive`, form);
  }
}
