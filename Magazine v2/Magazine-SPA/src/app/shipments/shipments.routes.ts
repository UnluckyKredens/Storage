import { Routes } from '@angular/router';
import { permissionGuard } from '../auth/guards/permission.guard';

export const shipmentsRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./containers/shipments-container/shipments-container.component').then(
        (m) => m.ShipmentsContainerComponent,
      ),
  },
  {
    path: 'receive',
    canActivate: [permissionGuard],
    data: { permission: 'stock-documents.receive' },
    loadComponent: () =>
      import('./containers/receive-shipment/receive-shipment.component').then(
        (m) => m.ReceiveShipmentComponent,
      ),
  },
  {
    path: 'create-shipment',
    canActivate: [permissionGuard],
    data: { permission: 'stock-shipments.create' },
    loadComponent: () =>
      import('./containers/create-shipment/create-shipment.component').then(
        (m) => m.CreateShipmentComponent,
      ),
  },
  {
    path: 'receipts',
    canActivate: [permissionGuard],
    data: { documentType: 1, permission: 'stock-documents.read' },
    loadComponent: () =>
      import('./containers/stock-documents/stock-documents.component').then(
        (m) => m.StockDocumentsComponent,
      ),
  },
  {
    path: 'issues',
    canActivate: [permissionGuard],
    data: { documentType: 2, permission: 'stock-documents.read' },
    loadComponent: () =>
      import('./containers/stock-documents/stock-documents.component').then(
        (m) => m.StockDocumentsComponent,
      ),
  },
  {
    path: 'history',
    canActivate: [permissionGuard],
    data: { permission: 'stock-documents.read' },
    loadComponent: () =>
      import('./containers/shipments-history/shipments-history.component').then(
        (m) => m.ShipmentsHistoryComponent,
      ),
  },
];
