import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';
import { StockDocumentFormDialogComponent } from '../stock-documents/stock-document-form-dialog.component';
import { StockDocumentPageData, StockDocumentType } from '../stock-documents/stock-document.models';
import { StockDocumentService } from '../stock-documents/stock-document.service';
import { NotificationService } from '../../../shared/services/notification.service';

@Component({
  selector: 'app-create-shipment',
  imports: [MatButtonModule, RouterLink],
  templateUrl: './create-shipment.component.html',
  styleUrl: './create-shipment.component.scss',
})
export class CreateShipmentComponent implements OnInit {
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);
  private readonly service = inject(StockDocumentService);
  private readonly notification = inject(NotificationService);

  pageData: StockDocumentPageData | null = null;
  loading = false;

  ngOnInit(): void {
    this.loading = true;
    this.service.getShipmentPageData().subscribe({
      next: (data) => {
        this.pageData = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać danych wysyłki.');
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  openForm(): void {
    if (!this.pageData) return;
    this.dialog
      .open(StockDocumentFormDialogComponent, {
        width: '980px',
        maxWidth: '96vw',
        data: {
          type: StockDocumentType.Shipment,
          document: null,
          pageData: this.pageData,
          employeeShipment: true,
        },
      })
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success(
          'Szkic WZ został utworzony i oczekuje na zatwierdzenie kierownika.',
        );
        this.cdr.markForCheck();
      });
  }
}
