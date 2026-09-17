import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  SaveStockDocument,
  StockDocumentDetails,
  StockDocumentPage,
  StockDocumentPageData,
  StockDocumentType,
} from './stock-document.models';

@Injectable({ providedIn: 'root' })
export class StockDocumentService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiUrl}/StockDocuments`;

  getPage(
    type: StockDocumentType,
    page: number,
    pageSize: number,
    search: string,
    sortBy: string,
    order: 'asc' | 'desc',
  ): Observable<StockDocumentPage> {
    const params = new HttpParams()
      .set('type', type)
      .set('page', page)
      .set('pageSize', pageSize)
      .set('search', search)
      .set('sortBy', sortBy)
      .set('order', order);
    return this.http.get<StockDocumentPage>(this.endpoint, { params });
  }

  getById(id: string): Observable<StockDocumentDetails> {
    return this.http.get<StockDocumentDetails>(`${this.endpoint}/${id}`);
  }

  scan(id: string): Observable<StockDocumentDetails> {
    return this.http.get<StockDocumentDetails>(`${this.endpoint}/scan/${id}`);
  }

  receive(id: string): Observable<StockDocumentDetails> {
    return this.http.post<StockDocumentDetails>(`${this.endpoint}/${id}/receive`, {});
  }

  getHistory(
    type: StockDocumentType | null,
    page: number,
    pageSize: number,
    search: string,
    sortBy: string,
    order: 'asc' | 'desc',
  ): Observable<StockDocumentPage> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('search', search)
      .set('sortBy', sortBy)
      .set('order', order);
    if (type !== null) params = params.set('type', type);
    return this.http.get<StockDocumentPage>(`${this.endpoint}/history`, { params });
  }

  getPageData(): Observable<StockDocumentPageData> {
    return this.http.get<StockDocumentPageData>(`${this.endpoint}/page-data`);
  }

  getShipmentPageData(): Observable<StockDocumentPageData> {
    return this.http.get<StockDocumentPageData>(`${this.endpoint}/shipment-page-data`);
  }

  createShipment(document: SaveStockDocument): Observable<StockDocumentDetails> {
    return this.http.post<StockDocumentDetails>(`${this.endpoint}/shipments`, document);
  }

  save(document: SaveStockDocument, editingId: string | null): Observable<StockDocumentDetails> {
    return editingId
      ? this.http.put<StockDocumentDetails>(`${this.endpoint}/${editingId}`, document)
      : this.http.post<StockDocumentDetails>(this.endpoint, document);
  }

  complete(id: string): Observable<StockDocumentDetails> {
    return this.http.post<StockDocumentDetails>(`${this.endpoint}/${id}/complete`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.endpoint}/${id}`);
  }
}
