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
import { Location } from '../../models/management.models';
import { LocationModalComponent } from '../../modals/location/location-modal.component';
import { LocationService } from '../../services/location.service';

@Component({
  selector: 'app-locations-page',
  imports: [
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './locations-page.component.html',
  styleUrl: '../management-page.scss',
})
export class LocationsPageComponent implements OnInit, AfterViewInit {
  private readonly service = inject(LocationService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly displayedColumns = ['locationCode', 'warehouseName', 'description', 'actions'];
  readonly dataSource = new MatTableDataSource<Location>([]);
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
      this.account.permissionCodes().includes('warehouses.manage')
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

  showDetails(row: Location): void {
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
        this.notification.success('Lokalizacja dodana.');
        this.load();
      });
  }

  edit(row: Location): void {
    if (!this.canManage) return;
    this.openModal('form', row)
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success('Lokalizacja została zaktualizowana.');
        this.load();
      });
  }

  remove(row: Location): void {
    if (!this.canManage) return;
    this.openModal('delete', row)
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(row.id).subscribe({
          next: () => {
            this.notification.success('Lokalizacja usunięta.');
            this.load();
          },
          error: (error: HttpErrorResponse) => this.showError(error),
        });
      });
  }

  private openModal(mode: 'details' | 'form' | 'delete', row: Location | null) {
    let width = '720px';
    if (mode === 'details') width = '640px';
    if (mode === 'delete') width = '480px';

    return this.dialog.open(LocationModalComponent, {
      width,
      maxWidth: '95vw',
      data: { mode, row },
    });
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać lokalizacji.');
    this.loading = false;
  }
}
