import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageContainer } from './container/storage-container/storage-container';
import { CategoriesPresenter } from './presenter/categories-presenter/categories-presenter';
import { BrowserModule } from '@angular/platform-browser';
import { StorageRoutingModule } from "./storage-routing-module";
import { SharedModule } from '../../shared/shared-module';
import { AddCategoryModal } from './presenter/categories-presenter/modal/add-category-modal/add-category-modal';
import { ItemsPresenter } from './presenter/items-presenter/items-presenter';
import { AddItemModal } from './presenter/items-presenter/modal/add-item-modal/add-item-modal';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SellersPresenter } from './presenter/sellers-presenter/sellers-presenter';
import { StockPresenter } from './presenter/stock-presenter/stock-presenter';
import { PurchasesPresenter } from './presenter/purchases-presenter/purchases-presenter';



@NgModule({
  declarations: [
    StorageContainer,
    CategoriesPresenter,
    AddCategoryModal,
    ItemsPresenter,
    AddItemModal,
    SellersPresenter,
    StockPresenter,
    PurchasesPresenter,
  ],
  imports: [
    CommonModule,
    StorageRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule
]
})
export class StorageModule { }
