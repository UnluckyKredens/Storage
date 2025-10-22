import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Stock } from '../../../shared/entities/stock.model';
import { apiEndpoints } from '../../../core/http/api.endpoints';

@Injectable({
  providedIn: 'root'
})
export class StockService {
  constructor(private readonly http: HttpClient) {}

  getStock(): Observable<Stock[]> {
    return this.http.get<Stock[]>(apiEndpoints.stock.stockUrl);
  }
}
