import { MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { CategoryService } from '../../../../services/category.service';
import { Component, Inject } from '@angular/core';
import { Category } from '../../../../../../shared/entities/category.model';

@Component({
  selector: 'app-add-category-modal',
  standalone: false,
  templateUrl: './add-category-modal.html',
  styleUrl: './add-category-modal.scss'
})
export class AddCategoryModal {

  constructor(private readonly categoryService: CategoryService, private dialog: MatDialog, @Inject(MAT_DIALOG_DATA) public data: {category: Category}) {}

  prevName: string = ''
  newName: string = ''

  ngOnInit(): void {
    if(this.data) {
    this.prevName = this.data.category.name
    }
  }

  addCategory(name: string) {
    this.categoryService.addCategory(name).subscribe(() => this.dialog.closeAll())
  }
  editCategory(newName: string) {
    var category = {id: this.data.category.id, name: newName}
    this.categoryService.editCategory(category).subscribe(() => this.dialog.closeAll())
  }
}
