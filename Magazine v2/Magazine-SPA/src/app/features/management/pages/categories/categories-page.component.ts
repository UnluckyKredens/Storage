import { Component, inject } from '@angular/core';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { CategoryModalComponent } from '../../modals/category/category-modal.component';
import { Category } from '../../../../shared/models/master-data.models';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-categories-page',
  imports: entityListPageImports,
  templateUrl: './categories-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class CategoriesPageComponent extends EntityListPage<Category> {
  protected readonly service = inject(CategoryService);
  protected readonly config = {
    managePermission: 'dictionaries.manage',
    modal: CategoryModalComponent,
    added: 'Kategoria dodana.',
    updated: 'Kategoria została zaktualizowana.',
    removed: 'Kategoria usunięta.',
    loadError: 'Nie udało się pobrać kategorii.',
  };
  readonly displayedColumns = ['name', 'description', 'actions'];
}
