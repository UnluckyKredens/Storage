import { Routes } from '@angular/router';
import { permissionGuard } from '../auth/guards/permission.guard';

export const storefrontRoutes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'packing' },
  {
    path: 'packing',
    canActivate: [permissionGuard],
    data: { permission: 'shipments.read' },
    loadComponent: () =>
      import('./pages/storefront-packing/storefront-packing-page.component').then(
        (m) => m.StorefrontPackingPageComponent,
      ),
  },
  {
    path: 'orders',
    canActivate: [permissionGuard],
    data: { permission: 'shipments.read' },
    loadComponent: () =>
      import('./pages/storefront-orders/storefront-orders-page.component').then(
        (m) => m.StorefrontOrdersPageComponent,
      ),
  },
];
