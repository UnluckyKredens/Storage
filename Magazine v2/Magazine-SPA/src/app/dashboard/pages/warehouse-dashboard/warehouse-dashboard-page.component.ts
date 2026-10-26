import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { catchError, finalize, forkJoin, of, switchMap } from 'rxjs';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { PurchaseOrderService } from '../../../purchase-orders/services/purchase-order.service';
import { ReplenishmentOrderModalComponent } from '../../components/replenishment-order-modal/replenishment-order-modal.component';
import { ProductSalesStatsComponent } from '../../components/product-sales-stats/product-sales-stats.component';
import { WarehouseFlowStatsComponent } from '../../components/warehouse-flow-stats/warehouse-flow-stats.component';
import { WarehouseDashboard } from '../../models/dashboard.models';
import { WarehouseDashboardService } from '../../services/warehouse-dashboard.service';

@Component({
  selector: 'app-warehouse-dashboard-page',
  imports: [
    DecimalPipe,
    MatButtonModule,
    MatProgressBarModule,
    MatTableModule,
    ProductSalesStatsComponent,
    TableNavigationComponent,
    WarehouseFlowStatsComponent,
  ],
  templateUrl: './warehouse-dashboard-page.component.html',
  styleUrls: [
    '../../../shared/styles/management-page.scss',
    './warehouse-dashboard-page.component.scss',
  ],
})
export class WarehouseDashboardPageComponent implements OnInit {
  private readonly service = inject(WarehouseDashboardService);
  private readonly account = inject(AccountService);
  private readonly notification = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly purchaseOrders = inject(PurchaseOrderService);
  readonly opening = signal(false);

  readonly alertColumns = [
    'warehouseName',
    'productName',
    'availableQuantity',
    'thresholds',
    'message',
  ];
  readonly dashboard = signal<WarehouseDashboard | null>(null);
  readonly loading = signal(false);
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) this.load();
    });
  }

  get canSeeFlow(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('shipments.approve')
    );
  }

  get canOrder(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('purchase-orders.create') ||
      this.account.permissionCodes().includes('shipments.create')
    );
  }

  createReplenishmentOrder(): void {
    const dashboard = this.dashboard();
    if (!dashboard || this.opening()) return;
    this.opening.set(true);
    this.purchaseOrders
      .getPageData()
      .pipe(finalize(() => this.opening.set(false)))
      .subscribe({
        next: (pageData) => {
          const alerts = dashboard.alerts.filter((alert) => alert.warehouseId === pageData.warehouse.id);
          const hubName = pageData.hubWarehouseName;
          const mode = pageData.warehouse.isHub || !hubName ? 'purchase' : 'request';
          if (mode === 'purchase' && pageData.suppliers.length === 0) {
            this.notification.error('Brak dostawców. Dodaj kontrahenta typu Dostawca.');
            return;
          }
          const backorders = dashboard.backorders.filter((item) => item.warehouseId === pageData.warehouse.id);
          if (alerts.length === 0 && backorders.length === 0) {
            this.notification.error('Brak alertów w aktywnym magazynie.');
            return;
          }
          this.dialog
            .open(ReplenishmentOrderModalComponent, {
              width: '820px',
              maxWidth: '96vw',
              data: { mode, hubName, pageData, alerts, backorders },
            })
            .afterClosed()
            .subscribe((created) => {
              if (!created) return;
              this.notification.success(
                mode === 'request' ? 'Prośba o wysyłkę została złożona.' : 'Zamówienie zostało utworzone.',
              );
            });
        },
        error: (error: HttpErrorResponse) =>
          this.notification.error(error.error?.message ?? 'Nie udało się pobrać danych zamówienia.'),
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
    this.service
      .get()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (dashboard) => this.dashboard.set(dashboard),
        error: (error: HttpErrorResponse) => {
          this.notification.error(error.error?.message ?? 'Nie udało się pobrać dashboardu.');
        },
      });
  }
}
