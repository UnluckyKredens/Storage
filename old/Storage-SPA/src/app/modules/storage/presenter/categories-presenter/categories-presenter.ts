import { BehaviorSubject, filter, map, Observable } from 'rxjs';
import { ChangeDetectorRef, Component } from '@angular/core';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../../../shared/entities/category.model';
import { MatDialog } from '@angular/material/dialog';
import { AddCategoryModal } from './modal/add-category-modal/add-category-modal';
import { MatTableDataSource } from '@angular/material/table';



@Component({
  selector: 'app-categories-presenter',
  standalone: false,
  templateUrl: './categories-presenter.html',
  styleUrl: './categories-presenter.scss'
})


export class CategoriesPresenter {
  constructor(private categoryService: CategoryService, private dialog: MatDialog) {
    this.getAllCategories()
    this.dialog.afterAllClosed.subscribe(x => {
      this.getAllCategories()
    })
  }

  displayedColumns: string[] = ['id', 'name', 'options'];
  categories = new MatTableDataSource()
  dataSource: BehaviorSubject<Category[]> = new BehaviorSubject<Category[]>([])
  ngOnInit(): void {
  }

  find(event: Event) {
    const filterEvent = (event.target as HTMLInputElement);
    this.dataSource.subscribe(x => {
      this.categories.filter = filterEvent.value.toLowerCase()
    })
  }

  getAllCategories() {
    this.categoryService.getAllCategories().subscribe(cat => {
      // this.dataSource.next([...cat])
      this.categories.data = [...cat]
    })
  }



  deleteCategory(category: Category) {
    var req = this.categoryService.deleteCategory(category)
    req.subscribe(() => {
      this.getAllCategories()
    })
  }

  editCategory(category: Category) {
    this.dialog.open(AddCategoryModal, {
      data: {category},
      width: '30%',
      height: '30%'
    })
  }

  openAddCategory() {
    this.dialog.open(AddCategoryModal, {
      width: '30%',
      height: '30%'
    })
  }
}
