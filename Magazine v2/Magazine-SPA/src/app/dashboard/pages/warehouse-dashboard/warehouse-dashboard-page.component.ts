import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { catchError, finalize, forkJoin, of, switchMap } from 'rxjs';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { WarehouseDashboard } from '../../models/dashboard.models';
import { WarehouseDashboardService } from '../../services/warehouse-dashboard.service';

@Component({
  selector: 'app-warehouse-dashboard-page',
  imports: [DecimalPipe, MatProgressBarModule, MatTableModule, TableNavigationComponent],
  templateUrl: './warehouse-dashboard-page.component.html',
  styleUrls: ['../../../management/pages/management-page.scss', './warehouse-dashboard-page.component.scss'],
})
export class WarehouseDashboardPageComponent implements OnInit {
  private readonly service = inject(WarehouseDashboardService);
  private readonly account = inject(AccountService);
  private readonly notification = inject(NotificationService);

  readonly alertColumns = ['warehouseName', 'productName', 'availableQuantity', 'thresholds', 'message'];
  readonly dashboard = signal<WarehouseDashboard | null>(null);
  readonly loading = signal(false);
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) this.load();
    });
  }

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin([this.account.load(), this.account.loadPermissions()])
      .pipe(
        switchMap(([user]) =>
          user.roleId === administratorRoleId
            ? this.account.loadWarehouses().pipe(catchError(() => of([])))
            : of([]),
        ),
        switchMap(() => this.service.get()),
        finalize(() => {
          this.loading.set(false);
          this.initialized = true;
        }),
      )
      .subscribe({
        next: (dashboard) => {
          this.dashboard.set(dashboard);
        },
        error: (error: HttpErrorResponse) => {
          this.notification.error(error.error?.message ?? 'Nie udało się pobrać dashboardu.');
        },
      });
  }

  load(): void {
    this.loading.set(true);
    this.service.get().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (dashboard) => this.dashboard.set(dashboard),
      error: (error: HttpErrorResponse) => {
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać dashboardu.');
      },
    });
  }
}
