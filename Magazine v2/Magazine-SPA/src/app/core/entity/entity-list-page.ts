import { HttpErrorResponse } from '@angular/common/http';
import { ComponentType } from '@angular/cdk/portal';
import { AfterViewInit, Directive, effect, inject, OnInit, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Observable } from 'rxjs';
import { administratorRoleId } from '../auth/roles';
import { AccountService } from '../auth/services/account.service';
import { TableNavigationComponent } from '../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../shared/services/notification.service';

export const entityListPageImports = [
  MatButtonModule,
  MatPaginatorModule,
  MatSortModule,
  MatTableModule,
  TableNavigationComponent,
];

export interface EntityListPageService<T> {
  getAll(): Observable<T[]>;
  getById(id: string): Observable<T>;
  delete(id: string): Observable<void>;
}

export interface EntityListPageConfig {
  managePermission: string;
  modal: ComponentType<unknown>;
  added: string;
  updated: string;
  removed: string;
  loadError: string;
}

@Directive()
export abstract class EntityListPage<T extends { id: string }> implements OnInit, AfterViewInit {
  protected abstract readonly service: EntityListPageService<T>;
  protected abstract readonly config: EntityListPageConfig;
  abstract readonly displayedColumns: string[];

  protected readonly reloadOnWarehouseChange: boolean = false;

  protected readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly dataSource = new MatTableDataSource<T>([]);
  loading = false;
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized && this.reloadOnWarehouseChange) this.load();
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
      this.account.permissionCodes().includes(this.config.managePermission)
    );
  }

  canEdit(_row: T): boolean {
    return this.canManage;
  }

  canDelete(_row: T): boolean {
    return this.canManage;
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

  showDetails(row: T): void {
    this.service.getById(row.id).subscribe({
      next: (details) => this.openModal('details', details),
      error: (error: HttpErrorResponse) => this.showError(error),
    });
  }

  add(): void {
    if (!this.canManage) return;
    this.openModal('form', null)
      .afterClosed()
      .subscribe((saved) => saved && this.reloadWithMessage(this.config.added));
  }

  edit(row: T): void {
    if (!this.canEdit(row)) return;
    this.openModal('form', row)
      .afterClosed()
      .subscribe((saved) => saved && this.reloadWithMessage(this.config.updated));
  }

  remove(row: T): void {
    if (!this.canDelete(row)) return;
    this.openModal('delete', row)
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(row.id).subscribe({
          next: () => this.reloadWithMessage(this.config.removed),
          error: (error: HttpErrorResponse) => this.showError(error),
        });
      });
  }

  private reloadWithMessage(message: string): void {
    this.notification.success(message);
    this.load();
  }

  private openModal(mode: 'details' | 'form' | 'delete', row: T | null) {
    const widths = { details: '640px', form: '720px', delete: '480px' };
    return this.dialog.open(this.config.modal, {
      width: widths[mode],
      maxWidth: '95vw',
      data: { mode, row },
    });
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? this.config.loadError);
    this.loading = false;
  }
}
