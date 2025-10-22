import { MatDialog } from '@angular/material/dialog';
import { SellersService } from './../../services/sellers.service';
import { Seller } from './../../../../shared/entities/seller.moder';
import { Component } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { AddSellerModal } from './modal/add-seller-modal/add-seller-modal';

@Component({
  selector: 'app-sellers-presenter',
  standalone: false,
  templateUrl: './sellers-presenter.html',
  styleUrl: './sellers-presenter.scss'
})
export class SellersPresenter {

  displayedColumns: string[] = ['id', 'name', 'nip', 'adress', 'options'];
  sellers = new MatTableDataSource(([]) as Seller[]);

  constructor(private sellersService: SellersService, private readonly dialog: MatDialog) {
    this.getAllSellers();
  }

  getAllSellers() {
    this.sellersService.getAllSellers().subscribe(s => {
      this.sellers.data = s
    })
  }

  addSeller() {
    this.dialog.open(AddSellerModal, {
      width: '30%',
      height: '50%'
    })
  }

  deleteSeller(seller: Seller) {
    this.sellersService.deleteSeller(seller).subscribe(() => {
      // console.log('Seller deleted');
      this.getAllSellers();
    })
  }

  editSeller(seller: Seller) {
    this.dialog.open(AddSellerModal, {
      width: '30%',
      height: '50%',
      data: {seller}
    })
  }

      find(event: Event) {
    const filterEvent = (event.target as HTMLInputElement);
      this.sellers.filter = filterEvent.value.toLowerCase()
  }

  public nil() {}

}

