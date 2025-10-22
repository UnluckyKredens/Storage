import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from './material.module';
import { HttpClient, HttpClientModule } from '@angular/common/http';



@NgModule({
  declarations: [],
  imports: [
    MaterialModule,
    HttpClientModule
  ],
  exports: [
    MaterialModule,
    HttpClientModule
  ]
})
export class SharedModule { }
