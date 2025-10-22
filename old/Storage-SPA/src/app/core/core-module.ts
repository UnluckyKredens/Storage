import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MainLayout } from './layouts/main-layout/main-layout';
import { Navbar } from './layouts/navbar/navbar';
import { SharedModule } from '../shared/shared-module';
import { AppRoutingModule } from "../app-routing-module";
import { BrowserModule } from '@angular/platform-browser';



@NgModule({
  declarations: [
    MainLayout,
    Navbar
  ],
  imports: [
    CommonModule,
    SharedModule,
    AppRoutingModule,
    BrowserModule
]
})
export class CoreModule { }
