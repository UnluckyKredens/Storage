import { Routes } from '@angular/router';
import { authGuard } from './core/auth/guards/auth.guard';
import { permissionGuard } from './core/auth/guards/permission.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () =>
      import('./core/auth/containers/login-container-component/login-container-component.component').then(
        (m) => m.LoginContainerComponentComponent,
      ),
  },
  {
    path: 'main',
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    loadComponent: () =>
      import('./core/layouts/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
      {
        path: 'dashboard',
        canActivate: [permissionGuard],
        data: { permission: 'inventory.read' },
        loadChildren: () =>
          import('./features/dashboard/dashboard.routes').then((m) => m.dashboardRoutes),
      },
      {
        path: 'magazine',
        redirectTo: 'inventory',
      },
      {
        path: 'inventory',
        loadChildren: () =>
          import('./features/inventory/inventory.routes').then((m) => m.inventoryRoutes),
      },
      {
        path: 'shipments',
        canActivate: [permissionGuard],
        data: { permission: 'shipments.read' },
        loadChildren: () =>
          import('./features/shipments/shipments.routes').then((m) => m.shipmentRoutes),
      },
      {
        path: 'purchase-orders',
        loadChildren: () =>
          import('./features/purchase-orders/purchase-orders.routes').then(
            (m) => m.purchaseOrderRoutes,
          ),
      },
      {
        path: 'storefront',
        loadChildren: () =>
          import('./features/storefront/storefront.routes').then((m) => m.storefrontRoutes),
      },
      {
        path: 'management',
        loadChildren: () =>
          import('./features/management/management.routes').then((m) => m.managementRoutes),
      },
      {
        path: 'account',
        loadComponent: () =>
          import('./core/auth/containers/account-settings/account-settings.component').then(
            (m) => m.AccountSettingsComponent,
          ),
      },
      {
        path: '**',
        redirectTo: 'dashboard',
      },
    ],
  },
  {
    path: '',
    redirectTo: 'main/dashboard',
    pathMatch: 'full',
  },
  {
    path: '**',
    redirectTo: 'main/dashboard',
  },
];
