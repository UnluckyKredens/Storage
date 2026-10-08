import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = localStorage.getItem('token');

  if (!token) {
    return next(request);
  }

  const activeWarehouseId = localStorage.getItem('activeWarehouseId');
  const headers: Record<string, string> = { Authorization: `Bearer ${token}` };
  if (activeWarehouseId) headers['X-Warehouse-Id'] = activeWarehouseId;

  const authenticatedRequest = request.clone({ setHeaders: headers });

  return next(authenticatedRequest);
};
