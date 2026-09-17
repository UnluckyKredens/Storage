import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, effect, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { AccountService } from '../../../auth/services/account.service';
import {
  FormSelectComponent,
  FormSelectOption,
} from '../../../shared/components/form-select/form-select.component';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { StockDocumentDetailsDialogComponent } from '../stock-documents/stock-document-details-dialog.component';
import { StockDocumentListItem, StockDocumentType } from '../stock-documents/stock-document.models';
import { StockDocumentService } from '../stock-documents/stock-document.service';

@Component({
  selector: 'app-shipments-history',
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    FormSelectComponent,
    TableNavigationComponent,
  ],
  templateUrl: './shipments-history.component.html',
  styleUrl: './shipments-history.component.scss',
})
export class ShipmentsHistoryComponent implements OnInit {
  private readonly account = inject(AccountService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);
  private readonly service = inject(StockDocumentService);
  private readonly notification = inject(NotificationService);

  readonly type = StockDocumentType;
  readonly documentTypes: FormSelectOption[] = [
    { value: StockDocumentType.Receipt, label: 'Przyjęcia PZ' },
    { value: StockDocumentType.Shipment, label: 'Wydania WZ' },
  ];
  readonly displayedColumns = [
    'number',
    'type',
    'warehouse',
    'destination',
    'approvedBy',
    'items',
    'completed',
    'actions',
  ];

  rows: StockDocumentListItem[] = [];
  documentType: StockDocumentType | null = null;
  pageIndex = 0;
  pageSize = 10;
  total = 0;
  search = '';
  sortBy = 'completed';
  order: 'asc' | 'desc' = 'desc';
  loading = false;
  private initialized = false;
  private previousWarehouseId = this.account.activeWarehouseId();

  constructor() {
    effect(() => {
      const warehouseId = this.account.activeWarehouseId();
      if (this.initialized && warehouseId !== this.previousWarehouseId) {
        this.previousWarehouseId = warehouseId;
        this.pageIndex = 0;
        this.load();
      }
    });
  }

  ngOnInit(): void {
    this.load();
    this.initialized = true;
  }

  load(): void {
    this.loading = true;
    this.service
      .getHistory(
        this.documentType,
        this.pageIndex + 1,
        this.pageSize,
        this.search,
        this.sortBy,
        this.order,
      )
      .subscribe({
        next: (result) => {
          this.rows = result.list;
          this.total = result.total;
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => this.handleError(error),
      });
  }

  changeType(): void {
    this.pageIndex = 0;
    this.load();
  }

  searchRows(value: string): void {
    this.search = value;
    this.pageIndex = 0;
    this.load();
  }

  changePage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  sortChanged(sort: Sort): void {
    this.sortBy = sort.active;
    this.order = sort.direction || 'asc';
    this.pageIndex = 0;
    this.load();
  }

  showDetails(row: StockDocumentListItem): void {
    this.service.getById(row.id).subscribe({
      next: (document) =>
        this.dialog.open(StockDocumentDetailsDialogComponent, {
          width: '820px',
          maxWidth: '95vw',
          data: document,
        }),
      error: (error: HttpErrorResponse) => this.handleError(error),
    });
  }

  private handleError(error: HttpErrorResponse): void {
    const message =
      error.status === 403
        ? 'Nie masz dostępu do historii wybranego magazynu.'
        : (error.error?.message ?? 'Nie udało się pobrać historii.');
    this.notification.error(message);
    this.loading = false;
    this.cdr.markForCheck();
  }
}
