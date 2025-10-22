import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../../shared/components/dialog-header/dialog-header.component';
import {
  CloseDialogActionsComponent,
  DeleteDialogBodyComponent,
  FormDialogActionsComponent,
} from '../../../../shared/components/dialog-parts/dialog-parts.component';
import { EntityDialog } from '../../../../core/entity/entity-dialog';
import { FormCheckboxComponent } from '../../../../shared/components/form-checkbox/form-checkbox.component';
import { FormInputComponent } from '../../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../../shared/components/form-textarea/form-textarea.component';
import { SearchSelectComponent } from '../../../../shared/components/search-select/search-select.component';
import { Product, SelectOption } from '../../../../shared/models/master-data.models';
import { ProductService } from '../../../../shared/services/product.service';

@Component({
  selector: 'app-product-modal',
  imports: [
    FormsModule,
    MatDialogModule,
    DialogHeaderComponent,
    CloseDialogActionsComponent,
    DeleteDialogBodyComponent,
    FormDialogActionsComponent,
    FormCheckboxComponent,
    FormInputComponent,
    FormTextareaComponent,
    SearchSelectComponent,
  ],
  templateUrl: './product-modal.component.html',
  styleUrls: ['../../../../shared/styles/management-modal.scss', './product-modal.component.scss'],
})
export class ProductModalComponent extends EntityDialog<Product> implements OnInit {
  private readonly service = inject(ProductService);

  form = {
    name: String(this.data.row?.name ?? ''),
    sku: String(this.data.row?.sku ?? ''),
    barcode: String(this.data.row?.barcode ?? ''),
    categoryId: String(this.data.row?.categoryId ?? ''),
    unitOfMeasureId: String(this.data.row?.unitOfMeasureId ?? ''),
    purchasePrice: Number(this.data.row?.purchasePrice ?? 0),
    salePrice: Number(this.data.row?.salePrice ?? 0),
    minimumQuantity: Number(this.data.row?.minimumQuantity ?? 0),
    optimumQuantity:
      this.data.row?.optimumQuantity === null || this.data.row?.optimumQuantity === undefined
        ? null
        : Number(this.data.row.optimumQuantity),
    description: String(this.data.row?.description ?? ''),
    imageUrl: String(this.data.row?.imageUrl ?? ''),
    isActive: this.data.row?.isActive === undefined ? true : Boolean(this.data.row.isActive),
  };
  categories: SelectOption[] = [];
  units: SelectOption[] = [];

  protected readonly titles = {
    details: 'Szczegóły produktu',
    delete: 'Usuń produkt',
    edit: 'Edytuj produkt',
    add: 'Dodaj produkt',
  };

  protected request() {
    return this.service.save(this.data.row?.productId ?? null, this.form);
  }

  ngOnInit(): void {
    if (this.data.mode !== 'form') return;
    this.service.getFormOptions().subscribe({
      next: (options) => {
        this.categories = options.categories;
        this.units = options.units;
      },
      error: () => this.notification.error('Nie udało się pobrać kategorii i jednostek miary.'),
    });
  }
}
