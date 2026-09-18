import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormSelectComponent } from '../../../shared/components/form-select/form-select.component';
import { SearchSelectComponent } from '../../../shared/components/search-select/search-select.component';
import { Product, SelectOption, ShipmentProduct } from '../../models/management.models';
import { ProductService } from '../../services/product.service';
import { ShipmentFormItem, ShipmentService } from '../../services/shipment.service';

interface ShipmentModalData {
  sourceWarehouseName: string;
  destinations?: SelectOption[];
  availableProducts?: ShipmentProduct[];
  mode?: 'create' | 'request';
}

interface ShipmentDraftItem extends ShipmentFormItem {
  productId: string;
  barcode: string;
  productName: string;
  sku: string;
  availableQuantity: number;
  lookupMode: 'barcode' | 'product';
}

@Component({
  selector: 'app-shipment-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatProgressBarModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormSelectComponent,
    SearchSelectComponent,
  ],
  templateUrl: './shipment-modal.component.html',
  styleUrls: ['../management-modal.scss', './shipment-modal.component.scss'],
})
export class ShipmentModalComponent implements OnInit {
  readonly data = inject<ShipmentModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(ShipmentService);
  private readonly productService = inject(ProductService);
  private readonly dialogRef = inject(MatDialogRef<ShipmentModalComponent, boolean>);
  readonly modalTitle = this.data?.mode === 'request' ? 'Prośba o wysyłkę' : 'Dodaj wysyłkę';

  destinationWarehouseId = '';
  barcode = '';
  selectedProductId = '';
  quantity = 1;
  productOptions: SelectOption[] = [];
  items: ShipmentDraftItem[] = [];
  saving = signal(false);
  loadingProduct = signal(false);
  loadingOptions = signal(false);
  error = '';

  ngOnInit(): void {
    if (!this.isRequestMode) {
      this.destinationWarehouseId = String(this.data.destinations?.[0]?.value ?? '');
    }
    this.loadProducts();
  }

  get isRequestMode(): boolean {
    return this.data.mode === 'request';
  }

  get submitLabel(): string {
    if (this.saving()) return 'Zapisywanie...';
    return this.isRequestMode ? 'Wyślij prośbę' : 'Utwórz';
  }

  get hasBarcode(): boolean {
    return this.barcode.trim().length > 0;
  }

  get hasSelectedProduct(): boolean {
    return this.selectedProductId.trim().length > 0;
  }

  barcodeChanged(value: string | number | null): void {
    this.barcode = String(value ?? '');
    if (this.hasBarcode) this.selectedProductId = '';
  }

  productChanged(value: string | number | null): void {
    this.selectedProductId = String(value ?? '');
    if (this.hasSelectedProduct) this.barcode = '';
  }

  addItem(): void {
    const barcode = this.barcode.trim();
    const productId = this.selectedProductId.trim();
    if (barcode && productId) {
      this.error = 'Użyj kodu kreskowego albo wyboru produktu, nie obu naraz.';
      return;
    }
    if ((!barcode && !productId) || this.quantity <= 0) {
      this.error = 'Podaj kod kreskowy albo wybierz produkt oraz ilość większą od zera.';
      return;
    }

    this.error = '';
    this.loadingProduct.set(true);
    const lookupMode = barcode ? 'barcode' : 'product';
    const request = barcode
      ? this.service.getProductByBarcode(barcode)
      : this.service.getProduct(productId);

    request.subscribe({
      next: (product) => {
        this.loadingProduct.set(false);
        if (this.addProduct(product, lookupMode)) {
          this.barcode = '';
          this.selectedProductId = '';
          this.quantity = 1;
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loadingProduct.set(false);
        this.error = error.error?.message ?? 'Nie znaleziono produktu.';
      },
    });
  }

  removeItem(index: number): void {
    this.items.splice(index, 1);
  }

  save(): void {
    if (!this.isRequestMode && !this.destinationWarehouseId) {
      this.error = 'Wybierz magazyn docelowy.';
      return;
    }
    if (this.items.length === 0) {
      this.error = this.isRequestMode
        ? 'Dodaj co najmniej jedną pozycję zapotrzebowania.'
        : 'Wybierz magazyn docelowy i dodaj co najmniej jedną pozycję.';
      return;
    }
    const invalidItem = this.items.find(
      (item) => !this.isRequestMode && item.quantity > item.availableQuantity,
    );
    if (invalidItem) {
      this.error = `Brak wystarczającej ilości dla kodu ${invalidItem.barcode}.`;
      return;
    }

    this.saving.set(true);
    this.error = '';
    const request = this.isRequestMode
      ? this.service.createRequest({
          items: this.items.map((item) => this.toFormItem(item)),
        })
      : this.service.create({
          destinationWarehouseId: this.destinationWarehouseId,
          items: this.items.map((item) => this.toFormItem(item)),
        });

    request.subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.error = error.error?.message ?? 'Nie udało się zapisać wysyłki.';
      },
    });
  }

  private loadProducts(): void {
    if (!this.isRequestMode) {
      const availableProducts = this.data.availableProducts ?? [];
      this.productOptions = availableProducts.map((product) => ({
        value: product.productId,
        label: this.availableProductLabel(product),
      }));
      if (availableProducts.length === 0) {
        this.error = 'Brak produktów dostępnych do wysyłki w magazynie źródłowym.';
      }
      return;
    }

    this.loadingOptions.set(true);
    this.productService.getAll(1, 100, '', 'name', 'asc').subscribe({
      next: (page) => {
        this.productOptions = page.list
          .filter((product) => product.isActive)
          .map((product) => ({
            value: product.productId,
            label: this.productLabel(product),
          }));
        this.loadingOptions.set(false);
      },
      error: () => {
        this.productOptions = [];
        this.loadingOptions.set(false);
        this.error = 'Nie udało się pobrać listy produktów.';
      },
    });
  }

  private addProduct(product: ShipmentProduct, lookupMode: 'barcode' | 'product'): boolean {
    if (!this.isRequestMode && product.availableQuantity <= 0) {
      this.error = `Produkt ${product.name} nie ma dostępnego stanu w tym magazynie.`;
      return false;
    }
    if (!this.isRequestMode && this.quantity > product.availableQuantity) {
      this.error = `Dostępne jest tylko ${product.availableQuantity} szt. dla kodu ${product.barcode}.`;
      return false;
    }

    const existing = this.items.find((item) => item.productId === product.productId);
    if (existing) {
      if (!this.isRequestMode && existing.quantity + this.quantity > product.availableQuantity) {
        this.error = `Dostępne jest tylko ${product.availableQuantity} szt. dla kodu ${product.barcode}.`;
        return false;
      }
      existing.quantity += this.quantity;
      existing.availableQuantity = product.availableQuantity;
      return true;
    }

    this.items.push({
      productId: product.productId,
      productName: product.name,
      sku: product.sku,
      barcode: product.barcode,
      availableQuantity: product.availableQuantity,
      quantity: this.quantity,
      lookupMode,
    });
    return true;
  }

  private toFormItem(item: ShipmentDraftItem): ShipmentFormItem {
    return item.lookupMode === 'barcode'
      ? { barcode: item.barcode, quantity: item.quantity }
      : { productId: item.productId, quantity: item.quantity };
  }

  private productLabel(product: Product): string {
    const barcode = product.barcode ? ` / ${product.barcode}` : '';
    return `${product.name} / ${product.sku}${barcode}`;
  }

  private availableProductLabel(product: ShipmentProduct): string {
    return `${product.name} / ${product.sku} / ${product.barcode} / dostępne: ${product.availableQuantity}`;
  }
}
