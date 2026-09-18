import { Routes } from '@angular/router';
import { permissionGuard } from '../auth/guards/permission.guard';

export const managementRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./containers/management-container/management-container.component').then(
        (m) => m.ManagementContainerComponent,
      ),
  },
  { path: 'items', redirectTo: 'products' },
  { path: 'employees', redirectTo: 'users' },
  { path: 'branches', redirectTo: 'warehouses' },
  {
    path: 'products',
    canActivate: [permissionGuard],
    data: { permission: 'products.read' },
    loadComponent: () =>
      import('./pages/products/products-page.component').then((m) => m.ProductsPageComponent),
  },
  {
    path: 'categories',
    canActivate: [permissionGuard],
    data: { permission: 'dictionaries.manage' },
    loadComponent: () =>
      import('./pages/categories/categories-page.component').then((m) => m.CategoriesPageComponent),
  },
  {
    path: 'units',
    canActivate: [permissionGuard],
    data: { permission: 'dictionaries.manage' },
    loadComponent: () =>
      import('./pages/units/units-page.component').then((m) => m.UnitsPageComponent),
  },
  {
    path: 'warehouses',
    canActivate: [permissionGuard],
    data: { permission: 'warehouses.read' },
    loadComponent: () =>
      import('./pages/warehouses/warehouses-page.component').then((m) => m.WarehousesPageComponent),
  },
  {
    path: 'locations',
    canActivate: [permissionGuard],
    data: { permission: 'warehouses.read' },
    loadComponent: () =>
      import('./pages/locations/locations-page.component').then((m) => m.LocationsPageComponent),
  },
  {
    path: 'inventory',
    canActivate: [permissionGuard],
    data: { permission: 'inventory.read' },
    loadComponent: () =>
      import('./pages/inventory/inventory-page.component').then((m) => m.InventoryPageComponent),
  },
  {
    path: 'stock-movements',
    canActivate: [permissionGuard],
    data: { permission: 'inventory.read' },
    loadComponent: () =>
      import('./pages/stock-movements/stock-movements-page.component').then(
        (m) => m.StockMovementsPageComponent,
      ),
  },
  {
    path: 'warehouse-operations',
    canActivate: [permissionGuard],
    data: { permission: 'inventory.read' },
    loadComponent: () =>
      import('./pages/warehouse-operations/warehouse-operations-page.component').then(
        (m) => m.WarehouseOperationsPageComponent,
      ),
  },
  {
    path: 'shipments',
    canActivate: [permissionGuard],
    data: { permission: 'shipments.read' },
    loadChildren: () => import('./shipments.routes').then((m) => m.shipmentRoutes),
  },
  {
    path: 'contractors',
    canActivate: [permissionGuard],
    data: { permission: 'contractors.read' },
    loadComponent: () =>
      import('./pages/contractors/contractors-page.component').then(
        (m) => m.ContractorsPageComponent,
      ),
  },
  {
    path: 'users',
    canActivate: [permissionGuard],
    data: { permission: 'users.read' },
    loadComponent: () =>
      import('./pages/users/users-page.component').then((m) => m.UsersPageComponent),
  },
  {
    path: 'roles',
    canActivate: [permissionGuard],
    data: { permission: 'roles.manage' },
    loadComponent: () =>
      import('./pages/roles/roles-page.component').then((m) => m.RolesPageComponent),
  },
  {
    path: 'permissions',
    canActivate: [permissionGuard],
    data: { permission: 'roles.manage' },
    loadComponent: () =>
      import('./pages/permissions/permissions-page.component').then(
        (m) => m.PermissionsPageComponent,
      ),
  },
  {
    path: 'audit-logs',
    canActivate: [permissionGuard],
    data: { permission: 'roles.manage' },
    loadComponent: () =>
      import('./pages/audit-logs/audit-logs-page.component').then((m) => m.AuditLogsPageComponent),
  },
];
