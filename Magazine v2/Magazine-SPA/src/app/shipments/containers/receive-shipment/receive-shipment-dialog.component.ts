import { DatePipe } from '@angular/common';
import { AfterViewInit, Component, inject, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import {
  StockDocumentDetails,
  StockDocumentStatus,
} from '../stock-documents/stock-document.models';

@Component({
  selector: 'app-receive-shipment-dialog',
  imports: [
    DatePipe,
    MatButtonModule,
    MatDialogModule,
    MatSortModule,
    MatTableModule,
    DialogHeaderComponent,
  ],
  templateUrl: './receive-shipment-dialog.component.html',
  styleUrl: '../stock-documents/stock-document-dialogs.component.scss',
})
export class ReceiveShipmentDialogComponent implements AfterViewInit {
  readonly document = inject<StockDocumentDetails>(MAT_DIALOG_DATA);
  readonly canReceive = this.document.status === StockDocumentStatus.Draft;
  readonly displayedColumns = ['productName', 'sku', 'quantity'];
  readonly dataSource = new MatTableDataSource(this.document.items);

  @ViewChild(MatSort) sort!: MatSort;

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }
}
