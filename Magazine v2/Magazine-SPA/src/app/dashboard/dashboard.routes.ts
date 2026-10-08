import { Routes } from '@angular/router';

export const dashboardRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./pages/warehouse-dashboard/warehouse-dashboard-page.component').then(
        (m) => m.WarehouseDashboardPageComponent,
      ),
  },
];
