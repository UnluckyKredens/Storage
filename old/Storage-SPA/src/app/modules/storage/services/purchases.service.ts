import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { apiEndpoints } from '../../../core/http/api.endpoints';
import { Purchase } from '../../../shared/entities/purchase.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PurchasesService {

  constructor(private readonly http: HttpClient) { }

  getPurchases(): Observable<Purchase[]> {
    return this.http.get<Purchase[]>(apiEndpoints.purchase.purchaseUrl);
  }

}
