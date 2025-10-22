import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { apiEndpoints } from '../../../core/http/api.endpoints';

@Injectable({
  providedIn: 'root'
})
export class StatsService {
  constructor(private readonly http: HttpClient) { }

  getStats(): Observable<any> {
    return this.http.get<any>(apiEndpoints.statistics.all);
  }
}
