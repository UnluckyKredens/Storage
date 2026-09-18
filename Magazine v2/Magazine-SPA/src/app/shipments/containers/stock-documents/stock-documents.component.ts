import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, effect, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { StockDocumentConfirmDialogComponent } from './stock-document-confirm-dialog.component';
import { StockDocumentDetailsDialogComponent } from './stock-document-details-dialog.component';
import { StockDocumentFormDialogComponent } from './stock-document-form-dialog.component';
import {
  StockDocumentDetails,
  StockDocumentListItem,
  StockDocumentPageData,
  StockDocumentStatus,
  StockDocumentType,
} from './stock-document.models';
import { StockDocumentService } from './stock-document.service';

@Component({
  selector: 'app-stock-documents',
  imports: [
    DatePipe,
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './stock-documents.component.html',
  styleUrl: './stock-documents.component.scss',
})
export class StockDocumentsComponent implements OnInit {
  private readonly account = inject(AccountService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(StockDocumentService);
  private readonly notification = inject(NotificationService);

  readonly status = StockDocumentStatus;
  readonly documentType = Number(this.route.snapshot.data['documentType']) as StockDocumentType;
  readonly title = this.documentType === StockDocumentType.Receipt ? 'Przyjęcia PZ' : 'Wydania WZ';
  readonly addLabel =
    this.documentType === StockDocumentType.Receipt ? 'Dodaj przyjęcie' : 'Dodaj wysyłkę';
  readonly displayedColumns = [
    'number',
    'status',
    'warehouse',
    'destination',
    'items',
    'created',
    'actions',
  ];

  rows: StockDocumentListItem[] = [];
  pageData: StockDocumentPageData = {
    products: [],
    locations: [],
    warehouses: [],
    destinationWarehouses: [],
    contractors: [],
  };
  pageIndex = 0;
  pageSize = 10;
  total = 0;
  search = '';
  sortBy = 'created';
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
        this.loadPageData();
      }
    });
  }

  ngOnInit(): void {
    this.account.load().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    this.account.loadPermissions().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
    this.load();
    this.loadPageData();
    this.initialized = true;
  }

  get canManageDrafts(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('stock-documents.manage')
    );
  }

  get canApprove(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('stock-documents.approve')
    );
  }

  canApproveRow(row: StockDocumentListItem): boolean {
    if (!this.canApprove) return false;
    return this.documentType === StockDocumentType.Receipt
      ? row.status === StockDocumentStatus.Received
      : row.status === StockDocumentStatus.Draft;
  }

  load(): void {
    this.loading = true;
    this.service
      .getPage(
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

  add(): void {
    if (!this.canManageDrafts) return;
    if (this.pageData.warehouses.length === 0) {
      this.notification.error('Brak dostępnego magazynu lub lokalizacji.');
      return;
    }
    this.openForm(null);
  }

  edit(row: StockDocumentListItem): void {
    if (!this.canManageDrafts || row.status !== StockDocumentStatus.Draft) return;
    this.service.getById(row.id).subscribe({
      next: (document) => this.openForm(document),
      error: (error: HttpErrorResponse) => this.handleError(error),
    });
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

  complete(row: StockDocumentListItem): void {
    if (!this.canApproveRow(row)) return;
    this.dialog
      .open(StockDocumentConfirmDialogComponent, {
        data: {
          title: 'Zatwierdź dokument',
          message: `Zatwierdzenie dokumentu ${row.number} zmieni stan magazynowy. Kontynuować?`,
          confirmLabel: 'Zatwierdź',
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.complete(row.id).subscribe({
          next: () => {
            this.notification.success(
              'Dokument został zatwierdzony, a stan magazynowy zaktualizowany.',
            );
            this.load();
          },
          error: (error: HttpErrorResponse) => this.handleError(error),
        });
      });
  }

  remove(row: StockDocumentListItem): void {
    if (!this.canManageDrafts || row.status !== StockDocumentStatus.Draft) return;
    this.dialog
      .open(StockDocumentConfirmDialogComponent, {
        data: {
          title: 'Usuń szkic',
          message: `Czy na pewno usunąć dokument ${row.number}?`,
          confirmLabel: 'Usuń',
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(row.id).subscribe({
          next: () => {
            this.notification.success('Szkic dokumentu został usunięty.');
            this.load();
          },
          error: (error: HttpErrorResponse) => this.handleError(error),
        });
      });
  }

  private openForm(document: StockDocumentDetails | null): void {
    this.dialog
      .open(StockDocumentFormDialogComponent, {
        width: '980px',
        maxWidth: '96vw',
        data: { type: this.documentType, document, pageData: this.pageData },
      })
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success(
          document ? 'Szkic został zaktualizowany.' : 'Szkic został utworzony.',
        );
        this.load();
      });
  }

  private loadPageData(): void {
    this.service.getPageData().subscribe({
      next: (data) => {
        this.pageData = data;
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => this.handleError(error),
    });
  }

  private handleError(error: HttpErrorResponse): void {
    const message =
      error.status === 403
        ? 'Nie masz dostępu do wybranego magazynu lub operacji.'
        : (error.error?.message ?? 'Nie udało się wykonać operacji.');
    this.notification.error(message);
    this.loading = false;
    this.cdr.markForCheck();
  }
}
