import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Item } from '../../../shared/entities/item.model';
import { apiEndpoints } from '../../../core/http/api.endpoints';

@Injectable({
  providedIn: 'root'
})
export class ItemsService {
  constructor(private readonly http: HttpClient) {}

  getAllItems(): Observable<Item[]> {
    return this.http.get<Item[]>(apiEndpoints.item.itemUrl)
  }

  addItem(item: Item): Observable<Item> {
   return this.http.post<Item>(apiEndpoints.item.itemUrl, item)
  }
  deleteItem(item: Item): Observable<void> {
    return this.http.delete<void>(`${apiEndpoints.item.itemUrl}`, { body: item });
  }

  updateItem(item: Item): Observable<Item> {
    return this.http.patch<Item>(`${apiEndpoints.item.itemUrl}/`, item);
  }
}
