import { Item } from './../../../../../../shared/entities/item.model';
import { ItemsPresenter } from './../../items-presenter';
import { Category } from './../../../../../../shared/entities/category.model';
import { Component, Inject } from '@angular/core';
import { CategoryService } from '../../../../services/category.service';
import { F } from '@angular/cdk/keycodes';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ItemsService } from '../../../../services/items.service';
import { MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-add-item-modal',
  standalone: false,
  templateUrl: './add-item-modal.html',
  styleUrl: './add-item-modal.scss'
})
export class AddItemModal {

  constructor(private categoryService: CategoryService, private itemService: ItemsService,private dialog: MatDialog, @Inject(MAT_DIALOG_DATA) private data: Item | undefined) {
    this.getCategories();
    this.setItem();
  }

  categories: Category[] = [];
  editedItem: Item | undefined;
  itemForm = new FormGroup({
    id: new FormControl<number | undefined>(undefined),
    name: new FormControl<string>('', [Validators.required]),
    sku: new FormControl<string>('', [Validators.required]),
    category: new FormControl<Category | null>(null, [Validators.required]),
    nettoPrice: new FormControl<number>(0.00, [Validators.required]),
    bruttoPrice: new FormControl<number>(0.00, [Validators.required]),
  });


  ngOnInit() {
    this.setItem();
  }

  getCategories() {
    this.categoryService.getAllCategories().subscribe(cats => {
     this.categories = cats;
    })

  }

  setItem() {
    if(this.data) {
      this.categoryService.getAllCategories().subscribe(cats => {
        this.categories = cats;
        this.editedItem = {...this.data, category: cats.find(c => c.name === this.data?.category.name)} as Item;
      if(this.editedItem) {
          this.itemForm.patchValue({
            id: this.editedItem.id,
            name: this.editedItem.name,
            sku: this.editedItem.sku,
            category: this.editedItem.category,
            nettoPrice: this.editedItem.nettoPrice,
            bruttoPrice: this.editedItem.bruttoPrice,
          });
        }
      })
    }
  }

  addItem() {
    var item: Item = {
      id: undefined!,
      name: this.itemForm.value.name!,
      sku: this.itemForm.value.sku!,
      category: this.itemForm.value.category!,
      nettoPrice: this.itemForm.value.nettoPrice!,
      bruttoPrice: this.itemForm.value.bruttoPrice!,
    };
    this.itemService.addItem(item).subscribe(() => {
      console.log('Item added successfully');
      this.dialog.closeAll();
    });
  }

  setNetto() {
    if(this.itemForm.value.bruttoPrice) {
      this.itemForm.patchValue({
        nettoPrice: (this.itemForm.value.bruttoPrice / 1.23).toFixed(2) as unknown as number
      });
    }
  }

  updateItem() {
    var body: Item = {
      id: this.itemForm.value.id!,
      name: this.itemForm.value.name!,
      sku: this.itemForm.value.sku!,
      category: this.itemForm.value.category!,
      nettoPrice: this.itemForm.value.nettoPrice!,
      bruttoPrice: this.itemForm.value.bruttoPrice!,
    };
    this.itemService.updateItem(body).subscribe(() => {
      console.log('Item updated successfully');
      this.dialog.closeAll();
    });
  }
}
