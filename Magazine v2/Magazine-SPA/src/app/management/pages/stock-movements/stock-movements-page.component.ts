import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { AfterViewInit, Component, inject, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { StockMovement } from '../../models/management.models';
import { StockMovementService } from '../../services/stock-movement.service';

@Component({
  selector: 'app-stock-movements-page',
  imports: [DatePipe, MatPaginatorModule, MatSortModule, MatTableModule, TableNavigationComponent],
  templateUrl: './stock-movements-page.component.html',
  styleUrl: '../management-page.scss',
})
export class StockMovementsPageComponent implements OnInit, AfterViewInit {
  private readonly service = inject(StockMovementService);
  private readonly notification = inject(NotificationService);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly displayedColumns = [
    'createdOnUtc',
    'type',
    'productName',
    'warehouseName',
    'locationCode',
    'quantityChange',
    'quantityAfter',
    'sourceNumber',
    'createdBy',
    'notes',
  ];
  readonly dataSource = new MatTableDataSource<StockMovement>([]);
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

  movementType(type: number): string {
    return (
      {
        1: 'Korekta ręczna',
        2: 'Transfer wyjście',
        3: 'Transfer wejście',
        4: 'Rezerwacja',
        5: 'Zwolnienie rezerwacji',
        6: 'Korekta',
        7: 'Inwentaryzacja',
      }[type] ?? `Typ ${type}`
    );
  }

  private showError(error: HttpErrorResponse): void {
    this.notification.error(error.error?.message ?? 'Nie udało się pobrać ruchów magazynowych.');
    this.loading = false;
  }
}
