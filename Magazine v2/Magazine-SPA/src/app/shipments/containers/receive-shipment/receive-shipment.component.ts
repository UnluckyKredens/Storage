import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { StockDocumentService } from '../stock-documents/stock-document.service';
import { ReceiveShipmentDialogComponent } from './receive-shipment-dialog.component';

@Component({
  selector: 'app-receive-shipment',
  imports: [FormsModule, FormInputComponent, MatButtonModule, RouterLink],
  templateUrl: './receive-shipment.component.html',
  styleUrl: './receive-shipment.component.scss',
})
export class ReceiveShipmentComponent {
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);
  private readonly service = inject(StockDocumentService);
  private readonly notification = inject(NotificationService);

  code = '';
  loading = false;

  scan(): void {
    const code = this.code.trim();
    if (!this.isGuid(code)) {
      this.notification.error('Kod przesyłki musi być poprawnym identyfikatorem GUID.');
      return;
    }

    this.loading = true;
    this.service.scan(code).subscribe({
      next: (document) => {
        this.loading = false;
        this.dialog
          .open(ReceiveShipmentDialogComponent, {
            width: '820px',
            maxWidth: '95vw',
            data: document,
          })
          .afterClosed()
          .subscribe((receive) => {
            if (receive) this.receive(document.id);
          });
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => this.handleError(error),
    });
  }

  private receive(id: string): void {
    this.loading = true;
    this.service.receive(id).subscribe({
      next: () => {
        this.loading = false;
        this.notification.success(
          'Przesyłka została przyjęta i oczekuje na zatwierdzenie kierownika.',
        );
        this.code = '';
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => this.handleError(error),
    });
  }

  private isGuid(value: string): boolean {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
  }

  private handleError(error: HttpErrorResponse): void {
    this.loading = false;
    this.notification.error(error.error?.message ?? 'Nie znaleziono przesyłki dla podanego kodu.');
    this.cdr.markForCheck();
  }
}
