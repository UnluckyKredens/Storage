import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { PurchaseOrderPageData } from '../../../management/models/management.models';
import { ShipmentService } from '../../../management/services/shipment.service';
import { PurchaseOrderService } from '../../../management/services/purchase-order.service';
import { WarehouseDashboardAlert, WarehouseDashboardBackorder } from '../../models/dashboard.models';

interface ReplenishmentModalData {
  mode: 'purchase' | 'request';
  hubName: string | null;
  pageData: PurchaseOrderPageData;
  alerts: WarehouseDashboardAlert[];
  backorders: WarehouseDashboardBackorder[];
}

interface ReplenishmentLine {
  productId: string;
  name: string;
  sku: string;
  available: number;
  backorder: number;
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
  private readonly shipments = inject(ShipmentService);
  private readonly dialogRef = inject(MatDialogRef<ReplenishmentOrderModalComponent, boolean>);

  contractorId = this.data.pageData.suppliers[0]?.id ?? '';
  readonly saving = signal(false);
  error = '';
  lines: ReplenishmentLine[] = this.buildLines();

  get totalValue(): number {
    return this.lines.reduce((sum, line) => sum + line.quantity * line.unitPrice, 0);
  }

  get canSave(): boolean {
    const hasSupplier = this.data.mode === 'request' || !!this.contractorId;
    return hasSupplier && this.lines.some((line) => line.quantity > 0) && !this.saving();
  }

  remove(line: ReplenishmentLine): void {
    this.lines = this.lines.filter((item) => item !== line);
  }

  save(): void {
    if (!this.canSave) return;
    this.saving.set(true);
    this.error = '';
    if (this.data.mode === 'request') {
      this.shipments
        .createRequest({
          items: this.lines
            .filter((line) => line.quantity > 0)
            .map((line) => ({ productId: line.productId, quantity: line.quantity })),
        })
        .subscribe({
          next: () => this.dialogRef.close(true),
          error: (error: HttpErrorResponse) => this.fail(error, 'Nie udało się złożyć prośby o wysyłkę.'),
        });
      return;
    }
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
        error: (error: HttpErrorResponse) => this.fail(error, 'Nie udało się utworzyć zamówienia.'),
      });
  }

  private fail(error: HttpErrorResponse, fallback: string): void {
    this.saving.set(false);
    this.error = error.error?.message ?? fallback;
  }

  private buildLines(): ReplenishmentLine[] {
    const products = new Map(this.data.pageData.products.map((product) => [product.productId, product]));
    const backorders = new Map<string, number>();
    if (this.data.mode === 'purchase') {
      for (const item of this.data.backorders) {
        backorders.set(item.productId, (backorders.get(item.productId) ?? 0) + item.quantity);
      }
    }

    const lines = new Map<string, ReplenishmentLine>();
    for (const alert of this.data.alerts) {
      const product = products.get(alert.productId);
      if (!product) continue;
      const target = alert.optimumQuantity ?? alert.minimumQuantity * 2;
      lines.set(product.productId, this.line(product, alert.availableQuantity, Math.max(target - alert.availableQuantity, 0), backorders));
    }
    for (const [productId] of backorders) {
      const product = products.get(productId);
      if (product && !lines.has(productId)) lines.set(productId, this.line(product, 0, 0, backorders));
    }
    return [...lines.values()].filter((line) => line.quantity > 0);
  }

  private line(
    product: PurchaseOrderPageData['products'][number],
    available: number,
    gap: number,
    backorders: Map<string, number>,
  ): ReplenishmentLine {
    const backorder = backorders.get(product.productId) ?? 0;
    return {
      productId: product.productId,
      name: product.name,
      sku: product.sku,
      available,
      backorder,
      quantity: Math.ceil((gap + backorder) * 1000) / 1000,
      unitPrice: product.purchasePrice,
    };
  }
}
