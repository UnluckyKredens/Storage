import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayout } from './core/layouts/main-layout/main-layout';

const routes: Routes = [
  {
    path: '*',
    redirectTo: 'storage/items',
    pathMatch: 'full'
  },
  {
    path: '',
    component: MainLayout,
    children: [
      {
        path:'storage',
        loadChildren: () => import('./modules/storage/storage-module').then(s => s.StorageModule)
      },
      {
        path: 'stats',
        loadChildren: () => import('./modules/stats/stats-module').then(s => s.StatsModule)
      }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
