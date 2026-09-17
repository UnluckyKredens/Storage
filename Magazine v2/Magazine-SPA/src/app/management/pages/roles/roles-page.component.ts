import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, inject, OnInit, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Role } from '../../models/management.models';
import { RoleModalComponent } from '../../modals/role/role-modal.component';
import { RoleService } from '../../services/role.service';

@Component({
  selector: 'app-roles-page',
  imports: [
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './roles-page.component.html',
  styleUrl: '../management-page.scss',
})
export class RolesPageComponent implements OnInit, AfterViewInit {
  private readonly service = inject(RoleService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly displayedColumns = ['name', 'permissionCodes', 'actions'];
  readonly dataSource = new MatTableDataSource<Role>([]);
  loading = false;

  ngOnInit(): void {
    this.account.load().subscribe();
    this.account.loadPermissions().subscribe();
    this.load();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  get canManage(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('roles.manage')
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

  showDetails(row: Role): void {
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
        this.notification.success('Rola dodana.');
        this.load();
      });
  }

  edit(row: Role): void {
    if (!this.canEdit(row)) return;
    this.openModal('form', row)
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success('Rola została zaktualizowana.');
        this.load();
      });
  }

  remove(row: Role): void {
    if (!this.canDelete(row)) return;
    this.openModal('delete', row)
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(row.id).subscribe({
          next: () => {
            this.notification.success('Rola usunięta.');
            this.load();
          },
          error: (error: HttpErrorResponse) => this.showError(error),
        });
      });
  }

  private openModal(mode: 'details' | 'form' | 'delete', row: Role | null) {
    let width = '720px';
    if (mode === 'details') width = '640px';
    if (mode === 'delete') width = '480px';

    return this.dialog.open(RoleModalComponent, {
      width,
      maxWidth: '95vw',
      data: { mode, row },
    });
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać ról.');
    this.loading = false;
  }

  canEdit(row: Role): boolean {
    return this.canManage && row.id !== administratorRoleId;
  }

  canDelete(row: Role): boolean {
    return this.canManage && row.id !== administratorRoleId;
  }
}
