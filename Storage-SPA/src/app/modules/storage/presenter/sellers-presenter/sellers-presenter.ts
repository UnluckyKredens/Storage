import { SellersService } from './../../services/sellers.service';
import { Seller } from './../../../../shared/entities/seller.moder';
import { Component } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-sellers-presenter',
  standalone: false,
  templateUrl: './sellers-presenter.html',
  styleUrl: './sellers-presenter.scss'
})
export class SellersPresenter {

  displayedColumns: string[] = ['id', 'name', 'nip', 'adress', 'options'];
  sellers = new MatTableDataSource(([]) as Seller[]);

  constructor(private sellersService: SellersService) {
    this.getAllSellers();
  }

  getAllSellers() {
    this.sellersService.getAllSellers().subscribe(s => {
      this.sellers.data = s
    })
  }

  public nil() {}

}

