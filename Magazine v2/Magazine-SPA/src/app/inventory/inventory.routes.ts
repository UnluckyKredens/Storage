import { Routes } from '@angular/router';
import { permissionGuard } from '../auth/guards/permission.guard';

export const inventoryRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [permissionGuard],
    data: { permission: 'inventory.read' },
    loadComponent: () =>
      import('./pages/inventory/inventory-page.component').then((m) => m.InventoryPageComponent),
  },
  {
    path: 'movements',
    canActivate: [permissionGuard],
    data: { permission: 'inventory.read' },
    loadComponent: () =>
      import('./pages/stock-movements/stock-movements-page.component').then(
        (m) => m.StockMovementsPageComponent,
      ),
  },
  {
    path: 'operations',
    canActivate: [permissionGuard],
    data: { permission: 'inventory.read' },
    loadComponent: () =>
      import('./pages/warehouse-operations/warehouse-operations-page.component').then(
        (m) => m.WarehouseOperationsPageComponent,
      ),
  },
];
