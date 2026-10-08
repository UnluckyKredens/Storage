import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OverallContainer } from './container/overall-container/overall-container';
import { StatsRoutingModule } from './stats-routing.module';
import { OverallPresenter } from './presenter/overall-presenter/overall-presenter';



@NgModule({
  declarations: [
    OverallContainer,
    OverallPresenter
  ],
  imports: [
    CommonModule,
    StatsRoutingModule
  ]
})
export class StatsModule { }
