import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexGrid,
  ApexLegend,
  ApexPlotOptions,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { finalize } from 'rxjs';
import { AccountService } from '../../../auth/services/account.service';
import { ProductSales, ProductSalesStats } from '../../models/dashboard.models';
import { WarehouseDashboardService } from '../../services/warehouse-dashboard.service';

@Component({
  selector: 'app-product-sales-stats',
  imports: [ChartComponent, DecimalPipe, MatProgressBarModule],
  templateUrl: './product-sales-stats.component.html',
  styleUrl: './product-sales-stats.component.scss',
})
export class ProductSalesStatsComponent {
  private readonly service = inject(WarehouseDashboardService);
  private readonly account = inject(AccountService);

  readonly periods = [30, 90, 365];
  readonly days = signal(30);
  readonly stats = signal<ProductSalesStats | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');

  private static readonly soldColor = '#1f5fbf';
  private static readonly stockColor = '#94a3b8';

  readonly bestSeries = computed<ApexAxisChartSeries>(() => [
    { name: 'Sprzedano', data: (this.stats()?.bestSellers ?? []).map((item) => item.soldQuantity) },
  ]);
  readonly worstSeries = computed<ApexAxisChartSeries>(() => [
    { name: 'Sprzedano', data: (this.stats()?.worstSellers ?? []).map((item) => item.soldQuantity) },
    { name: 'Na stanie', data: (this.stats()?.worstSellers ?? []).map((item) => item.stockQuantity) },
  ]);
  readonly bestXaxis = computed<ApexXAxis>(() => this.axis(this.stats()?.bestSellers ?? []));
  readonly worstXaxis = computed<ApexXAxis>(() => this.axis(this.stats()?.worstSellers ?? []));

  readonly bestColors = [ProductSalesStatsComponent.soldColor];
  readonly worstColors = [ProductSalesStatsComponent.soldColor, ProductSalesStatsComponent.stockColor];

  readonly chart: ApexChart = {
    type: 'bar',
    height: 300,
    fontFamily: 'inherit',
    foreColor: '#64748b',
    toolbar: { show: false },
    animations: { enabled: true, speed: 350 },
  };
  readonly plotOptions: ApexPlotOptions = {
    bar: { horizontal: true, barHeight: '62%', borderRadius: 4, borderRadiusApplication: 'end' },
  };
  readonly stroke: ApexStroke = { width: 1, colors: ['#ffffff'] };
  readonly dataLabels: ApexDataLabels = { enabled: false };
  readonly grid: ApexGrid = {
    borderColor: '#e2e8f0',
    xaxis: { lines: { show: true } },
    yaxis: { lines: { show: false } },
    padding: { left: 4, right: 12 },
  };
  readonly yaxis: ApexYAxis = {
    labels: { maxWidth: 170, style: { colors: '#0f172a', fontSize: '13px' } },
  };
  readonly legend: ApexLegend = {
    position: 'top',
    horizontalAlign: 'left',
    fontSize: '13px',
    markers: { size: 6, shape: 'square', offsetX: -2 },
  };
  readonly tooltip: ApexTooltip = {
    shared: true,
    intersect: false,
    y: {
      formatter: (value: number) =>
        new Intl.NumberFormat('pl-PL', { maximumFractionDigits: 3 }).format(value) + ' szt.',
    },
  };

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      const days = this.days();
      untracked(() => this.load(days));
    });
  }

  selectPeriod(days: number): void {
    this.days.set(days);
  }

  private axis(items: ProductSales[]): ApexXAxis {
    return {
      categories: items.map((item) => item.productName),
      labels: { formatter: (value: string) => `${Math.round(Number(value))}`, style: { colors: '#64748b' } },
      axisBorder: { show: false },
      axisTicks: { show: false },
    };
  }

  private load(days: number): void {
    this.loading.set(true);
    this.error.set('');
    this.service
      .getSales(days)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (stats) => this.stats.set(stats),
        error: (error: HttpErrorResponse) =>
          this.error.set(error.error?.message ?? 'Nie udało się pobrać statystyk sprzedaży.'),
      });
  }
}
