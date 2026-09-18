import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLog } from '../models/management.models';

export interface AuditLogFilters {
  entityName?: string | null;
  entityId?: string | null;
  limit?: number;
}

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/AuditLogs`;

  getAll(filters: AuditLogFilters = {}): Observable<AuditLog[]> {
    let params = new HttpParams();
    if (filters.entityName) params = params.set('entityName', filters.entityName);
    if (filters.entityId) params = params.set('entityId', filters.entityId);
    if (filters.limit) params = params.set('limit', filters.limit);

    return this.http.get<AuditLog[]>(this.apiUrl, { params });
  }
}
