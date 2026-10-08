import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, effect, inject, OnInit, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { InventoryItem } from '../../models/management.models';
import { InventoryModalComponent } from '../../modals/inventory/inventory-modal.component';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-inventory-page',
  imports: [
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './inventory-page.component.html',
  styleUrl: '../management-page.scss',
})
export class InventoryPageComponent implements OnInit, AfterViewInit {
  private readonly service = inject(InventoryService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly displayedColumns = [
    'productName',
    'warehouseName',
    'locationCode',
    'quantity',
    'reservedQuantity',
    'availableQuantity',
    'actions',
  ];
  readonly dataSource = new MatTableDataSource<InventoryItem>([]);
  loading = false;
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) this.load();
    });
  }

  ngOnInit(): void {
    this.account.load().subscribe();
    this.account.loadPermissions().subscribe();
    this.load();
    this.initialized = true;
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  get canManage(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('inventory.manage')
    );
  }

  load(): void {
    this.loading = true;
    this.service.getAll().subscribe({
      next: (rows) => {
        this.dataSource.data = rows;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => this.showError(error),
    });
  }

  search(value: string): void {
    this.dataSource.filter = value.trim().toLowerCase();
    this.paginator.firstPage();
  }

  showDetails(row: InventoryItem): void {
    this.service.getById(row.id).subscribe({
      next: (details) => this.openModal('details', details),
      error: (error: HttpErrorResponse) => this.showError(error),
    });
  }

  add(): void {
    if (!this.canManage) return;
    this.openModal('form', null)
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success('Stan magazynowy dodany.');
        this.load();
      });
  }

  edit(row: InventoryItem): void {
    if (!this.canManage) return;
    this.openModal('form', row)
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success('Stan magazynowy został zaktualizowany.');
        this.load();
      });
  }

  remove(row: InventoryItem): void {
    if (!this.canManage) return;
    this.openModal('delete', row)
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(row.id).subscribe({
          next: () => {
            this.notification.success('Stan magazynowy usunięty.');
            this.load();
          },
          error: (error: HttpErrorResponse) => this.showError(error),
        });
      });
  }

  private openModal(mode: 'details' | 'form' | 'delete', row: InventoryItem | null) {
    let width = '720px';
    if (mode === 'details') width = '640px';
    if (mode === 'delete') width = '480px';

    return this.dialog.open(InventoryModalComponent, {
      width,
      maxWidth: '95vw',
      data: { mode, row },
    });
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać stanów magazynowych.');
    this.loading = false;
  }
}
