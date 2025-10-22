import { Component, Input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-delete-dialog-body',
  imports: [MatButtonModule, MatDialogModule],
  host: { style: 'display: contents' },
  template: `
    <mat-dialog-content>{{ message }}</mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" [mat-dialog-close]="false">Anuluj</button>
      <button mat-flat-button type="button" [mat-dialog-close]="true">Usuń</button>
    </mat-dialog-actions>
  `,
})
export class DeleteDialogBodyComponent {
  @Input({ required: true }) message = '';
}

@Component({
  selector: 'app-form-dialog-actions',
  imports: [MatButtonModule, MatDialogModule],
  host: { style: 'display: contents' },
  template: `
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Anuluj</button>
      <button mat-flat-button type="submit" [disabled]="invalid || saving">
        {{ saving ? 'Zapisywanie...' : 'Zapisz' }}
      </button>
    </mat-dialog-actions>
  `,
})
export class FormDialogActionsComponent {
  @Input() saving = false;
  @Input() invalid: boolean | null = false;
}

@Component({
  selector: 'app-close-dialog-actions',
  imports: [MatButtonModule, MatDialogModule],
  host: { style: 'display: contents' },
  template: `
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Zamknij</button>
    </mat-dialog-actions>
  `,
})
export class CloseDialogActionsComponent {}
