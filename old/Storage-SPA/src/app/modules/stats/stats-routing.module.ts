import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { OverallContainer } from "./container/overall-container/overall-container";

const routes: Routes = [
  {
    path: '',
    component: OverallContainer
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StatsRoutingModule { }
