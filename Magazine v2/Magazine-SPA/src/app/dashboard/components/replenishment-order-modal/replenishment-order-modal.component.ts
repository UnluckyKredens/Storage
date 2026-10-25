import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { PurchaseOrderPageData } from '../../../management/models/management.models';
import { PurchaseOrderService } from '../../../management/services/purchase-order.service';
import { WarehouseDashboardAlert } from '../../models/dashboard.models';

interface ReplenishmentModalData {
  pageData: PurchaseOrderPageData;
  alerts: WarehouseDashboardAlert[];
}

interface ReplenishmentLine {
  productId: string;
  name: string;
  sku: string;
  available: number;
  quantity: number;
  unitPrice: number;
}

@Component({
  selector: 'app-replenishment-order-modal',
  imports: [DecimalPipe, FormsModule, MatButtonModule, MatDialogModule, DialogHeaderComponent],
  templateUrl: './replenishment-order-modal.component.html',
  styleUrl: './replenishment-order-modal.component.scss',
})
export class ReplenishmentOrderModalComponent {
  readonly data = inject<ReplenishmentModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(PurchaseOrderService);
  private readonly dialogRef = inject(MatDialogRef<ReplenishmentOrderModalComponent, boolean>);

  contractorId = this.data.pageData.suppliers[0]?.id ?? '';
  readonly saving = signal(false);
  error = '';
  lines: ReplenishmentLine[] = this.buildLines();

  get totalValue(): number {
    return this.lines.reduce((sum, line) => sum + line.quantity * line.unitPrice, 0);
  }

  get canSave(): boolean {
    return !!this.contractorId && this.lines.some((line) => line.quantity > 0) && !this.saving();
  }

  remove(line: ReplenishmentLine): void {
    this.lines = this.lines.filter((item) => item !== line);
  }

  save(): void {
    if (!this.canSave) return;
    this.saving.set(true);
    this.error = '';
    this.service
      .create({
        contractorId: this.contractorId,
        notes: 'Zamówienie uzupełniające z alertów magazynowych',
        items: this.lines
          .filter((line) => line.quantity > 0)
          .map((line) => ({ productId: line.productId, quantity: line.quantity, unitPrice: line.unitPrice })),
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error = error.error?.message ?? 'Nie udało się utworzyć zamówienia.';
        },
      });
  }

  // Order up to the optimum (twice the minimum when no optimum is set); rounded up to the 3 decimals the API accepts.
  private buildLines(): ReplenishmentLine[] {
    const products = new Map(this.data.pageData.products.map((product) => [product.productId, product]));
    return this.data.alerts
      .map((alert) => {
        const product = products.get(alert.productId);
        const target = alert.optimumQuantity ?? alert.minimumQuantity * 2;
        return product
          ? {
              productId: product.productId,
              name: product.name,
              sku: product.sku,
              available: alert.availableQuantity,
              quantity: Math.ceil((target - alert.availableQuantity) * 1000) / 1000,
              unitPrice: product.purchasePrice,
            }
          : null;
      })
      .filter((line): line is ReplenishmentLine => line !== null && line.quantity > 0);
  }
}
