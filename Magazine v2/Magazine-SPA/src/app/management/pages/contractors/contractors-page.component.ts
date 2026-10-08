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
import { Contractor } from '../../models/management.models';
import { ContractorModalComponent } from '../../modals/contractor/contractor-modal.component';
import { ContractorService } from '../../services/contractor.service';

@Component({
  selector: 'app-contractors-page',
  imports: [
    MatButtonModule,
    MatPaginatorModule,
    MatSortModule,
    MatTableModule,
    TableNavigationComponent,
  ],
  templateUrl: './contractors-page.component.html',
  styleUrl: '../management-page.scss',
})
export class ContractorsPageComponent implements OnInit, AfterViewInit {
  private readonly service = inject(ContractorService);
  private readonly account = inject(AccountService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly displayedColumns = ['name', 'taxNumber', 'type', 'email', 'phone', 'actions'];
  readonly dataSource = new MatTableDataSource<Contractor>([]);
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
      this.account.permissionCodes().includes('contractors.manage')
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

  showDetails(row: Contractor): void {
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
        this.notification.success('Kontrahent dodany.');
        this.load();
      });
  }

  edit(row: Contractor): void {
    if (!this.canManage) return;
    this.openModal('form', row)
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.notification.success('Kontrahent został zaktualizowany.');
        this.load();
      });
  }

  remove(row: Contractor): void {
    if (!this.canManage) return;
    this.openModal('delete', row)
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(row.id).subscribe({
          next: () => {
            this.notification.success('Kontrahent usunięty.');
            this.load();
          },
          error: (error: HttpErrorResponse) => this.showError(error),
        });
      });
  }

  private openModal(mode: 'details' | 'form' | 'delete', row: Contractor | null) {
    let width = '720px';
    if (mode === 'details') width = '640px';
    if (mode === 'delete') width = '480px';

    return this.dialog.open(ContractorModalComponent, {
      width,
      maxWidth: '95vw',
      data: { mode, row },
    });
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać kontrahentów.');
    this.loading = false;
  }

  typeLabel(type: number): string {
    if (type === 1) return 'Dostawca';
    if (type === 2) return 'Odbiorca';
    return 'Oba';
  }
}
