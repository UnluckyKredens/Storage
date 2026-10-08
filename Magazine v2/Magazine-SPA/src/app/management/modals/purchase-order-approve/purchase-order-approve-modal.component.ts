import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { PurchaseOrder } from '../../models/management.models';
import { PurchaseOrderService } from '../../services/purchase-order.service';

interface PurchaseOrderApproveModalData {
  order: PurchaseOrder;
}

@Component({
  selector: 'app-purchase-order-approve-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormTextareaComponent,
  ],
  templateUrl: './purchase-order-approve-modal.component.html',
  styleUrls: ['../management-modal.scss'],
})
export class PurchaseOrderApproveModalComponent {
  readonly data = inject<PurchaseOrderApproveModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(PurchaseOrderService);
  private readonly dialogRef = inject(MatDialogRef<PurchaseOrderApproveModalComponent, boolean>);

  invoiceNumber = '';
  paperDocumentNumber = '';
  notes = this.data.order.notes ?? '';
  saving = signal(false);
  error = '';

  save(): void {
    if (!this.invoiceNumber.trim() || !this.paperDocumentNumber.trim()) {
      this.error = 'Podaj numer faktury i numer dokumentu papierowego.';
      return;
    }

    this.saving.set(true);
    this.error = '';
    this.service
      .approve(this.data.order.id, {
        invoiceNumber: this.invoiceNumber.trim(),
        paperDocumentNumber: this.paperDocumentNumber.trim(),
        notes: this.notes.trim() || null,
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error = error.error?.message ?? 'Nie udało się zaakceptować zamówienia.';
        },
      });
  }
}
