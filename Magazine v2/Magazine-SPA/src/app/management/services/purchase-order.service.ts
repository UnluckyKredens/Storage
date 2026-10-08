import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PurchaseOrder,
  PurchaseOrderPageData,
} from '../models/management.models';

export interface PurchaseOrderFormItem {
  productId: string;
  quantity: number;
  unitPrice: number | null;
}

export interface PurchaseOrderForm {
  contractorId: string;
  items: PurchaseOrderFormItem[];
  notes: string | null;
}

export interface PurchaseOrderApproveForm {
  invoiceNumber: string;
  paperDocumentNumber: string;
  notes: string | null;
}

export interface PurchaseOrderReceiveForm {
  checkedItemIds: string[];
  notes: string | null;
}

@Injectable({ providedIn: 'root' })
export class PurchaseOrderService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/PurchaseOrders`;

  getAll(): Observable<PurchaseOrder[]> {
    return this.http.get<PurchaseOrder[]>(this.apiUrl);
  }

  getPending(): Observable<PurchaseOrder[]> {
    return this.http.get<PurchaseOrder[]>(`${this.apiUrl}/pending`);
  }

  getApproved(): Observable<PurchaseOrder[]> {
    return this.http.get<PurchaseOrder[]>(`${this.apiUrl}/approved`);
  }

  getPageData(): Observable<PurchaseOrderPageData> {
    return this.http.get<PurchaseOrderPageData>(`${this.apiUrl}/page-data`);
  }

  create(form: PurchaseOrderForm): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(this.apiUrl, form);
  }

  approve(id: string, form: PurchaseOrderApproveForm): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/approve`, form);
  }

  receive(id: string, form: PurchaseOrderReceiveForm): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/receive`, form);
  }
}
