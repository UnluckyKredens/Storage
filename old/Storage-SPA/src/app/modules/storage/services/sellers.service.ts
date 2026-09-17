import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Seller } from '../../../shared/entities/seller.moder';
import { apiEndpoints } from '../../../core/http/api.endpoints';

@Injectable({
  providedIn: 'root'
})
export class SellersService {
  constructor(private readonly http: HttpClient) { }

  getAllSellers(): Observable<Seller[]> {
    return this.http.get<Seller[]>(apiEndpoints.seller.sellerUrl);
  }

  addSeller(seller: Seller): Observable<Seller> {
    return this.http.post<Seller>(apiEndpoints.seller.sellerUrl, seller);
  }

  updateSeller(seller: Seller): Observable<Seller> {
    return this.http.patch<Seller>(`${apiEndpoints.seller.sellerUrl}`, seller);
  }

  deleteSeller(seller: Seller){
    return this.http.delete<void>(`${apiEndpoints.seller.sellerUrl}/`, {body: seller});

  }
}
