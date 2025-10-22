import { Component } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-purchases-presenter',
  standalone: false,
  templateUrl: './purchases-presenter.html',
  styleUrl: './purchases-presenter.scss'
})
export class PurchasesPresenter {

  displayedColumns: string[] = ['id', 'item', 'quantity', 'totalPrice', 'date', 'options'];
  purchases = new MatTableDataSource();
  constructor() {}
  nil() {}

}
