import { Routes } from '@angular/router';
import { authGuard } from './auth/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () =>
      import('./auth/containers/login-container-component/login-container-component.component').then(
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
        redirectTo: 'management',
      },
      {
        path: 'magazine',
        redirectTo: 'management/inventory',
      },
      {
        path: 'management',
        loadChildren: () =>
          import('./management/management.routes').then((m) => m.managementRoutes),
      },
      {
        path: 'shipments',
        loadChildren: () => import('./shipments/shipments.routes').then((m) => m.shipmentsRoutes),
      },
      {
        path: 'account',
        loadComponent: () =>
          import('./auth/containers/account-settings/account-settings.component').then(
            (m) => m.AccountSettingsComponent,
          ),
      },
    ],
  },
  {
    path: '',
    redirectTo: '',
    pathMatch: 'full',
  },
  {
    path: '**',
    redirectTo: '',
  },
];
