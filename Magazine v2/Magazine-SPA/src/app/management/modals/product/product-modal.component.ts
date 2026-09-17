import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormCheckboxComponent } from '../../../shared/components/form-checkbox/form-checkbox.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormSelectComponent } from '../../../shared/components/form-select/form-select.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Product, SelectOption } from '../../models/management.models';
import { ProductService } from '../../services/product.service';
import { SearchSelectComponent } from '../../../shared/components/search-select/search-select.component';

interface ProductModalData {
  mode: 'details' | 'form' | 'delete';
  row: Product | null;
}

@Component({
  selector: 'app-product-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormCheckboxComponent,
    FormInputComponent,
    FormSelectComponent,
    FormTextareaComponent,
    SearchSelectComponent
],
  templateUrl: './product-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class ProductModalComponent implements OnInit {
  readonly data = inject<ProductModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(ProductService);
  private readonly dialogRef = inject(MatDialogRef<ProductModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    name: String(this.data.row?.name ?? ''),
    sku: String(this.data.row?.sku ?? ''),
    barcode: String(this.data.row?.barcode ?? ''),
    categoryId: String(this.data.row?.categoryId ?? ''),
    unitOfMeasureId: String(this.data.row?.unitOfMeasureId ?? ''),
    purchasePrice: Number(this.data.row?.purchasePrice ?? 0),
    salePrice: Number(this.data.row?.salePrice ?? 0),
    description: String(this.data.row?.description ?? ''),
    isActive: this.data.row?.isActive === undefined ? true : Boolean(this.data.row.isActive),
  };
  saving = false;
  categories: SelectOption[] = [];
  units: SelectOption[] = [];

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

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły produktu';
    if (this.data.mode === 'delete') return 'Usuń produkt';
    return this.data.row ? 'Edytuj produkt' : 'Dodaj produkt';
  }

  save(): void {
    this.saving = true;

    this.service.save(this.data.row?.productId ?? null, this.form).subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: HttpErrorResponse) => {
        this.saving = false;
        this.notification.error(error.error?.message ?? 'Nie udało się zapisać danych.');
      },
    });
  }

  display(value: unknown): string {
    if (value === null || value === undefined || value === '') return '—';
    if (typeof value === 'boolean') return value ? 'Tak' : 'Nie';
    if (Array.isArray(value)) return value.join(', ') || '—';
    return String(value);
  }
}
