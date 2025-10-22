import { Component } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { PurchasesService } from '../../services/purchases.service';

@Component({
  selector: 'app-purchases-presenter',
  standalone: false,
  templateUrl: './purchases-presenter.html',
  styleUrl: './purchases-presenter.scss'
})
export class PurchasesPresenter {

  displayedColumns: string[] = ['id', 'item', 'quantity', 'cost', 'purchaseDate', 'options'];
  purchases = new MatTableDataSource();
  constructor(private readonly purchaseService: PurchasesService) {}

  getAllPurchases(): void {
    this.purchaseService.getPurchases().subscribe((purchases) => {
      this.purchases.data = purchases;
    });
  }

  ngOnInit(): void {
    this.getAllPurchases();
  }
  nil() {}

}
