import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { PurchaseOrder } from '../../models/management.models';
import { PurchaseOrderService } from '../../services/purchase-order.service';

interface PurchaseOrderReceiveModalData {
  order: PurchaseOrder;
}

@Component({
  selector: 'app-purchase-order-receive-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormTextareaComponent,
  ],
  templateUrl: './purchase-order-receive-modal.component.html',
  styleUrls: ['../management-modal.scss', './purchase-order-receive-modal.component.scss'],
})
export class PurchaseOrderReceiveModalComponent {
  readonly data = inject<PurchaseOrderReceiveModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(PurchaseOrderService);
  private readonly dialogRef = inject(MatDialogRef<PurchaseOrderReceiveModalComponent, boolean>);

  checkedItemIds = new Set<string>();
  notes = '';
  saving = signal(false);
  error = '';

  get allChecked(): boolean {
    return this.data.order.items.length > 0 &&
      this.data.order.items.every((item) => this.checkedItemIds.has(item.id));
  }

  toggleItem(itemId: string, checked: boolean): void {
    if (checked) this.checkedItemIds.add(itemId);
    else this.checkedItemIds.delete(itemId);
  }

  save(): void {
    if (!this.allChecked) {
      this.error = 'Potwierdź wszystkie pozycje z checklisty.';
      return;
    }

    this.saving.set(true);
    this.error = '';
    this.service
      .receive(this.data.order.id, {
        checkedItemIds: [...this.checkedItemIds],
        notes: this.notes.trim() || null,
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error = error.error?.message ?? 'Nie udało się przyjąć zamówienia.';
        },
      });
  }
}
