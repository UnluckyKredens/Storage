import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, inject, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { TableNavigationComponent } from '../../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../../shared/services/notification.service';
import { AuditLog } from '../../models/management.models';
import { AuditLogService } from '../../services/audit-log.service';

const actionLabels: Record<string, string> = {
  Accept: 'Przyjęcie do realizacji',
  Approve: 'Akceptacja',
  Cancel: 'Anulowanie',
  Claim: 'Przejęcie przez magazyn',
  Complete: 'Zaksięgowanie',
  Create: 'Utworzenie',
  Delete: 'Usunięcie',
  Dispatch: 'Wysłanie',
  MarkInTransit: 'W drodze',
  Pack: 'Spakowanie',
  Receive: 'Przyjęcie',
  Ship: 'Nadanie przesyłki',
  Update: 'Zmiana',
};

const entityLabels: Record<string, string> = {
  Inventory: 'Stan magazynowy',
  PurchaseOrder: 'Zamówienie zewnętrzne',
  Shipment: 'Wysyłka',
  StorefrontOrder: 'Zamówienie sklepu',
  WarehouseOperation: 'Dokument magazynowy',
};

@Component({
  selector: 'app-audit-logs-page',
  imports: [DatePipe, MatPaginatorModule, MatSortModule, MatTableModule, TableNavigationComponent],
  templateUrl: './audit-logs-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class AuditLogsPageComponent implements OnInit, AfterViewInit {
  private readonly service = inject(AuditLogService);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly displayedColumns = ['createdOnUtc', 'action', 'entityName', 'userName', 'summary'];
  readonly dataSource = new MatTableDataSource<AuditLog>([]);
  loading = false;
  searchValue = '';

  ngOnInit(): void {
    this.load();
  }

  constructor() {
    this.dataSource.filterPredicate = (row, filter) =>
      [
        this.actionLabel(row.action),
        this.entityLabel(row.entityName),
        row.action,
        row.entityName,
        row.userName,
        row.summary,
      ]
        .join(' ')
        .toLowerCase()
        .includes(filter);
    this.dataSource.sortingDataAccessor = (row, column) => {
      if (column === 'action') return this.actionLabel(row.action);
      if (column === 'entityName') return this.entityLabel(row.entityName);
      const value = row[column as keyof AuditLog];
      return typeof value === 'number' ? value : String(value ?? '');
    };
  }

  actionLabel(action: string): string {
    return actionLabels[action] ?? action;
  }

  entityLabel(entityName: string): string {
    return entityLabels[entityName] ?? entityName;
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
