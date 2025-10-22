import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { takeUntil } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AccountService } from '../services/account.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const api = new URL(environment.apiUrl, window.location.origin);
  const target = new URL(request.url, window.location.origin);
  if (target.origin !== api.origin || !(target.pathname === api.pathname || target.pathname.startsWith(`${api.pathname}/`))) {
    return next(request);
  }
  const account = inject(AccountService);
  const token = localStorage.getItem('token');
  const activeWarehouseId = localStorage.getItem('activeWarehouseId');
  const headers: Record<string, string> = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;
  if (activeWarehouseId) headers['X-Warehouse-Id'] = activeWarehouseId;
  const result = next(request.clone({ setHeaders: headers }));
  const scoped = /\/(Inventory|Locations|Shipments|PurchaseOrders|WarehouseDashboard|WarehouseOperations|StockMovements)(\/|$)|\/Storefront\/orders/.test(target.pathname);
  return scoped ? result.pipe(takeUntil(account.warehouseChanges)) : result;
};
