import { Component } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { Stock } from '../../../../shared/entities/stock.model';
import { StockService } from '../../services/stock.service';

@Component({
  selector: 'app-stock-presenter',
  standalone: false,
  templateUrl: './stock-presenter.html',
  styleUrl: './stock-presenter.scss'
})
export class StockPresenter {

  displayedColumns: string[] = ['id', 'item', 'category', 'quantity', 'options'];
  stock = new MatTableDataSource(([]) as Stock[]);

  constructor(private readonly stockService: StockService) {
    this.getStock();
  }
  nil() {}

  getStock() {
    this.stockService.getStock().subscribe(s => {
      this.stock.data = s
    })
  }
}
