import { Routes } from '@angular/router';

export const shipmentRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./containers/shipments-container/shipments-container.component').then(
        (m) => m.ShipmentsContainerComponent,
      ),
  },
  {
    path: 'all',
    data: { mode: 'all' },
    loadComponent: () =>
      import('./pages/shipments/shipments-page.component').then((m) => m.ShipmentsPageComponent),
  },
  {
    path: 'pending',
    data: { mode: 'pending' },
    loadComponent: () =>
      import('./pages/shipments/shipments-page.component').then((m) => m.ShipmentsPageComponent),
  },
  {
    path: 'ready',
    data: { mode: 'ready' },
    loadComponent: () =>
      import('./pages/shipments/shipments-page.component').then((m) => m.ShipmentsPageComponent),
  },
];
