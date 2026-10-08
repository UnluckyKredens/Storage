import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, inject, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { AuditLog } from '../../models/management.models';
import { AuditLogService } from '../../services/audit-log.service';

@Component({
  selector: 'app-audit-logs-page',
  imports: [DatePipe, MatPaginatorModule, MatSortModule, MatTableModule, TableNavigationComponent],
  templateUrl: './audit-logs-page.component.html',
  styleUrl: '../management-page.scss',
})
export class AuditLogsPageComponent implements OnInit, AfterViewInit {
  private readonly service = inject(AuditLogService);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly displayedColumns = [
    'createdOnUtc',
    'action',
    'entityName',
    'entityId',
    'userName',
    'summary',
  ];
  readonly dataSource = new MatTableDataSource<AuditLog>([]);
  loading = false;
  searchValue = '';

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  load(): void {
    this.loading = true;
    this.service.getAll({ limit: 1000 }).subscribe({
      next: (rows) => {
        this.dataSource.data = rows;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => this.showError(error),
    });
  }

  search(value: string): void {
    this.searchValue = value;
    this.dataSource.filter = value.trim().toLowerCase();
    this.paginator.firstPage();
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać audytu.');
    this.loading = false;
  }
}
