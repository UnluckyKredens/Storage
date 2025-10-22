import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Category } from '../../../shared/entities/category.model';
import { apiEndpoints } from '../../../core/http/api.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
    constructor(private readonly http: HttpClient) {}

    getAllCategories(): Observable<Category[]> {
      var res = this.http.get<Category[]>(apiEndpoints.category.categoryUrl)
      return res
    }

    addCategory(name: string): Observable<Category> {
      var req = {"name": name}
      return this.http.post<Category>(apiEndpoints.category.categoryUrl, req)
    }

    editCategory(category: Category) {
      return this.http.patch(apiEndpoints.category.categoryUrl, category)
    }

    deleteCategory(category: Category): Observable<void> {

      return this.http.delete<void>(apiEndpoints.category.categoryUrl, { body: category });

    }

}
