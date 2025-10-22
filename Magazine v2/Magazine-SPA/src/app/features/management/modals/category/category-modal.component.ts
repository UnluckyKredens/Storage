import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../../shared/components/dialog-header/dialog-header.component';
import {
  CloseDialogActionsComponent,
  DeleteDialogBodyComponent,
  FormDialogActionsComponent,
} from '../../../../shared/components/dialog-parts/dialog-parts.component';
import { EntityDialog } from '../../../../core/entity/entity-dialog';
import { FormInputComponent } from '../../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../../shared/components/form-textarea/form-textarea.component';
import { Category } from '../../../../shared/models/master-data.models';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-category-modal',
  imports: [
    FormsModule,
    MatDialogModule,
    DialogHeaderComponent,
    CloseDialogActionsComponent,
    DeleteDialogBodyComponent,
    FormDialogActionsComponent,
    FormInputComponent,
    FormTextareaComponent,
  ],
  templateUrl: './category-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class CategoryModalComponent extends EntityDialog<Category> {
  private readonly service = inject(CategoryService);

  form = {
    name: String(this.data.row?.name ?? ''),
    description: String(this.data.row?.description ?? ''),
  };

  protected readonly titles = {
    details: 'Szczegóły kategorii',
    delete: 'Usuń kategorię',
    edit: 'Edytuj kategorię',
    add: 'Dodaj kategorię',
  };

  protected request() {
    return this.service.save(this.data.row?.id ?? null, this.form);
  }
}
