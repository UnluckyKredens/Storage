import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { SearchSelectComponent } from '../../../shared/components/search-select/search-select.component';
import { PurchaseOrderPageData, PurchaseOrderProduct, SelectOption } from '../../models/management.models';
import { PurchaseOrderFormItem, PurchaseOrderService } from '../../services/purchase-order.service';

interface PurchaseOrderModalData {
  pageData: PurchaseOrderPageData;
}

interface PurchaseOrderDraftItem extends PurchaseOrderFormItem {
  productName: string;
  sku: string;
  barcode: string;
  totalPrice: number;
}

@Component({
  selector: 'app-purchase-order-modal',
  imports: [
    FormsModule,
    DecimalPipe,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormTextareaComponent,
    SearchSelectComponent,
  ],
  templateUrl: './purchase-order-modal.component.html',
  styleUrls: ['../management-modal.scss', './purchase-order-modal.component.scss'],
})
export class PurchaseOrderModalComponent {
  readonly data = inject<PurchaseOrderModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(PurchaseOrderService);
  private readonly dialogRef = inject(MatDialogRef<PurchaseOrderModalComponent, boolean>);

  contractorId = this.data.pageData.suppliers[0]?.id ?? '';
  selectedProductId = '';
  quantity = 1;
  unitPrice: number | null = null;
  notes = '';
  items: PurchaseOrderDraftItem[] = [];
  saving = signal(false);
  error = '';

  get supplierOptions(): SelectOption[] {
    return this.data.pageData.suppliers.map((supplier) => ({
      value: supplier.id,
      label: `${supplier.name} / NIP ${supplier.taxNumber}`,
    }));
  }

  get productOptions(): SelectOption[] {
    return this.data.pageData.products.map((product) => ({
      value: product.productId,
      label: `${product.name} / ${product.sku} / ${product.barcode}`,
    }));
  }

  get totalValue(): number {
    return this.items.reduce((sum, item) => sum + item.totalPrice, 0);
  }

  productChanged(value: string | number | null): void {
    this.selectedProductId = String(value ?? '');
    const product = this.findProduct(this.selectedProductId);
    this.unitPrice = product?.purchasePrice ?? null;
  }

  addItem(): void {
    const product = this.findProduct(this.selectedProductId);
    if (!product || this.quantity <= 0 || this.unitPrice === null || this.unitPrice < 0) {
      this.error = 'Wybierz produkt, ilość większą od zera i poprawną cenę.';
      return;
    }

    const existing = this.items.find((item) => item.productId === product.productId);
    if (existing) {
      existing.quantity += this.quantity;
      existing.unitPrice = this.unitPrice;
      existing.totalPrice = existing.quantity * existing.unitPrice;
    } else {
      this.items.push({
        productId: product.productId,
        productName: product.name,
        sku: product.sku,
        barcode: product.barcode,
        quantity: this.quantity,
        unitPrice: this.unitPrice,
        totalPrice: this.quantity * this.unitPrice,
      });
    }

    this.error = '';
    this.selectedProductId = '';
    this.quantity = 1;
    this.unitPrice = null;
  }

  removeItem(index: number): void {
    this.items.splice(index, 1);
  }

  save(): void {
    if (!this.contractorId) {
      this.error = 'Wybierz dostawcę.';
      return;
    }
    if (this.items.length === 0) {
      this.error = 'Dodaj co najmniej jedną pozycję zamówienia.';
      return;
    }

    this.saving.set(true);
    this.error = '';
    this.service
      .create({
        contractorId: this.contractorId,
        items: this.items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
        })),
        notes: this.notes.trim() || null,
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error = error.error?.message ?? 'Nie udało się utworzyć zamówienia.';
        },
      });
  }

  private findProduct(productId: string): PurchaseOrderProduct | undefined {
    return this.data.pageData.products.find((product) => product.productId === productId);
  }
}
