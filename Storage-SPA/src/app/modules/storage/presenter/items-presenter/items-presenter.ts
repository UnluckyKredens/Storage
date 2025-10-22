import { Item } from './../../../../shared/entities/item.model';
import { Component } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { ItemsService } from '../../services/items.service';
import { MatDialog } from '@angular/material/dialog';
import { AddItemModal } from './modal/add-item-modal/add-item-modal';

@Component({
  selector: 'app-items-presenter',
  standalone: false,
  templateUrl: './items-presenter.html',
  styleUrl: './items-presenter.scss'
})
export class ItemsPresenter {
  displayedColumns: string[] = ['id', 'name', 'sku', 'category', 'brutto', 'netto', 'options'];
  items = new MatTableDataSource()

  constructor(private itemsService: ItemsService, private dialog: MatDialog) {
    this.getItems()
    dialog.afterAllClosed.subscribe(() => {
      this.getItems()
    })
  }

    find(event: Event) {
    const filterEvent = (event.target as HTMLInputElement);
      this.items.filter = filterEvent.value.toLowerCase()
  }

  getItems() {
    this.itemsService.getAllItems().subscribe(i => {
      this.items.data = i
    })
  }
  nil() {}

  addItem() {
    this.dialog.open(AddItemModal, {
      width: '50%',
      height: '60%'
    }).afterClosed().subscribe(() => {
      this.getItems()
    })
  }

  editItem(item: Item) {
    this.dialog.open(AddItemModal, {
      data: item,
      width: '50%',
      height: '60%',
    })
  }

  deleteItem(Item: Item) {
   this.itemsService.deleteItem(Item).subscribe(() => {
    console.log('Item deleted');
    this.getItems()
   })
  }
}
