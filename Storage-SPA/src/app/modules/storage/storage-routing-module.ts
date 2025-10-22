import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StorageContainer } from './container/storage-container/storage-container';
import { CategoriesPresenter } from './presenter/categories-presenter/categories-presenter';
import { ItemsPresenter } from './presenter/items-presenter/items-presenter';
import { SellersPresenter } from './presenter/sellers-presenter/sellers-presenter';
import { StockPresenter } from './presenter/stock-presenter/stock-presenter';

const routes: Routes = [
  {
    path: '',
    component: StorageContainer,
    children: [
      {
        path: 'categories',
        component: CategoriesPresenter
      },
      {
        path:'items',
        component: ItemsPresenter
      },
      {
        path: 'sellers',
        component: SellersPresenter
      },
      {
        path: 'stock',
        component: StockPresenter
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StorageRoutingModule { }
