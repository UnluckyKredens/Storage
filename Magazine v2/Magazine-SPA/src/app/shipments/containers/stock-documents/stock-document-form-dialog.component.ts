import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import {
  FormSelectComponent,
  FormSelectOption,
} from '../../../shared/components/form-select/form-select.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { NotificationService } from '../../../shared/services/notification.service';
import {
  ContractorOption,
  LocationOption,
  SaveStockDocument,
  StockDocumentDetails,
  StockDocumentItemForm,
  StockDocumentPageData,
  StockDocumentType,
} from './stock-document.models';
import { StockDocumentService } from './stock-document.service';

export interface StockDocumentFormData {
  type: StockDocumentType;
  document: StockDocumentDetails | null;
  pageData: StockDocumentPageData;
  employeeShipment?: boolean;
}

@Component({
  selector: 'app-stock-document-form-dialog',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormSelectComponent,
    FormTextareaComponent,
  ],
  templateUrl: './stock-document-form-dialog.component.html',
  styleUrl: './stock-document-dialogs.component.scss',
})
export class StockDocumentFormDialogComponent {
  readonly data = inject<StockDocumentFormData>(MAT_DIALOG_DATA);
  private readonly service = inject(StockDocumentService);
  private readonly dialogRef = inject(MatDialogRef<StockDocumentFormDialogComponent, boolean>);
  private readonly notification = inject(NotificationService);

  warehouseId = this.data.document?.warehouseId ?? this.data.pageData.warehouses[0]?.id ?? '';
  contractorId = this.data.document?.contractorId ?? '';
  destinationWarehouseId = this.data.document?.destinationWarehouseId ?? '';
  notes = this.data.document?.notes ?? '';
  items: StockDocumentItemForm[] = this.data.document
    ? this.data.document.items.map((item) => ({
        productId: item.productId,
        locationId: item.locationId,
        quantity: item.quantity,
      }))
    : [this.emptyItem()];
  saving = false;

  get title(): string {
    const name = this.data.type === StockDocumentType.Receipt ? 'przyjęcie PZ' : 'wysyłkę WZ';
    return this.data.document ? `Edytuj ${name}` : `Dodaj ${name}`;
  }

  get contractors(): ContractorOption[] {
    return this.data.pageData.contractors.filter(
      (contractor) => contractor.type === 1 || contractor.type === 3,
    );
  }

  get warehouseOptions(): FormSelectOption[] {
    return this.data.pageData.warehouses.map((warehouse) => ({
      value: warehouse.id,
      label: warehouse.name,
    }));
  }

  get contractorOptions(): FormSelectOption[] {
    return this.contractors.map((contractor) => ({
      value: contractor.id,
      label: contractor.name,
    }));
  }

  get destinationWarehouseOptions(): FormSelectOption[] {
    return this.destinationWarehouses.map((warehouse) => ({
      value: warehouse.id,
      label: warehouse.name,
    }));
  }

  get productOptions(): FormSelectOption[] {
    return this.data.pageData.products.map((product) => ({
      value: product.id,
      label: `${product.name} (${product.sku})`,
    }));
  }

  get locationOptions(): FormSelectOption[] {
    return this.locations().map((location) => ({
      value: location.id,
      label: location.code,
    }));
  }

  get destinationWarehouses() {
    return this.data.pageData.destinationWarehouses.filter(
      (warehouse) => warehouse.id !== this.warehouseId,
    );
  }

  locations(): LocationOption[] {
    return this.data.pageData.locations.filter(
      (location) => location.warehouseId === this.warehouseId,
    );
  }

  changeWarehouse(): void {
    if (this.destinationWarehouseId === this.warehouseId) this.destinationWarehouseId = '';
    const availableIds = new Set(this.locations().map((location) => location.id));
    for (const item of this.items) {
      if (!availableIds.has(item.locationId)) item.locationId = '';
    }
  }

  addItem(): void {
    this.items.push(this.emptyItem());
  }

  removeItem(index: number): void {
    if (this.items.length > 1) this.items.splice(index, 1);
  }

  save(): void {
    const pairs = new Set(this.items.map((item) => `${item.productId}:${item.locationId}`));
    if (pairs.size !== this.items.length) {
      this.notification.error('Produkt w tej samej lokalizacji może wystąpić tylko raz.');
      return;
    }

    const request: SaveStockDocument = {
      id: this.data.document?.id ?? null,
      type: this.data.type,
      warehouseId: this.warehouseId,
      contractorId: this.data.type === StockDocumentType.Receipt ? this.contractorId : null,
      destinationWarehouseId:
        this.data.type === StockDocumentType.Shipment ? this.destinationWarehouseId : null,
      notes: this.notes.trim() || null,
      items: this.items,
    };

    this.saving = true;
    const saveRequest = this.data.employeeShipment
      ? this.service.createShipment(request)
      : this.service.save(request, this.data.document?.id ?? null);
    saveRequest.subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: HttpErrorResponse) => {
        this.saving = false;
        this.notification.error(error.error?.message ?? 'Nie udało się zapisać dokumentu.');
      },
    });
  }

  private emptyItem(): StockDocumentItemForm {
    return { productId: '', locationId: '', quantity: 1 };
  }
}
