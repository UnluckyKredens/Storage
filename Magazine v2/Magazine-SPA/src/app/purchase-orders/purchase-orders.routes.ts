import { Routes } from '@angular/router';
import { permissionGuard } from '../auth/guards/permission.guard';

export const purchaseOrderRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [permissionGuard],
    data: { permission: 'purchase-orders.read' },
    loadComponent: () =>
      import('./pages/purchase-orders/purchase-orders-page.component').then(
        (m) => m.PurchaseOrdersPageComponent,
      ),
  },
];
