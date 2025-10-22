import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { apiEndpoints } from '../../../core/http/api.endpoints';

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {

  constructor(private readonly http: HttpClient) { }

  checkConncection(): boolean {
    return this.http.get(apiEndpoints.connection.connectionUrl).subscribe({
      next: (res) => {
        return true;
      },
      error: (err) => {
        return false;
      }
    }) as unknown as boolean;
  }
}
