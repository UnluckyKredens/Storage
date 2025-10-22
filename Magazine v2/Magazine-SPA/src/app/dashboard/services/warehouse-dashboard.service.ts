import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WarehouseDashboard, WarehouseFlowStats } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class WarehouseDashboardService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/WarehouseDashboard`;

  get(): Observable<WarehouseDashboard> {
    return this.http.get<WarehouseDashboard>(this.apiUrl);
  }

  getFlow(days: number): Observable<WarehouseFlowStats> {
    return this.http.get<WarehouseFlowStats>(`${this.apiUrl}/flow`, { params: { days } });
  }
}
