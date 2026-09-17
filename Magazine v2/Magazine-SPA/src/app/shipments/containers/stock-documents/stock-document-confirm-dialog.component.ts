import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';

export interface StockDocumentConfirmData {
  title: string;
  message: string;
  confirmLabel: string;
}

@Component({
  selector: 'app-stock-document-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule, DialogHeaderComponent],
  template: `
    <app-dialog-header [title]="data.title" />
    <mat-dialog-content>{{ data.message }}</mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" [mat-dialog-close]="false">Anuluj</button>
      <button mat-flat-button type="button" [mat-dialog-close]="true">
        {{ data.confirmLabel }}
      </button>
    </mat-dialog-actions>
  `,
})
export class StockDocumentConfirmDialogComponent {
  readonly data = inject<StockDocumentConfirmData>(MAT_DIALOG_DATA);
}
