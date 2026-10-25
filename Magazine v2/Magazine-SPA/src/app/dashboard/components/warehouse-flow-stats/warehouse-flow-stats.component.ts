import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexLegend,
  ApexMarkers,
  ApexPlotOptions,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { finalize } from 'rxjs';
import { AccountService } from '../../../auth/services/account.service';
import { WarehouseFlowStats } from '../../models/dashboard.models';
import { WarehouseDashboardService } from '../../services/warehouse-dashboard.service';

@Component({
  selector: 'app-warehouse-flow-stats',
  imports: [ChartComponent, DatePipe, DecimalPipe, MatProgressBarModule],
  templateUrl: './warehouse-flow-stats.component.html',
  styleUrl: './warehouse-flow-stats.component.scss',
})
export class WarehouseFlowStatsComponent {
  private readonly service = inject(WarehouseDashboardService);
  private readonly account = inject(AccountService);

  readonly periods = [7, 30, 90];
  readonly days = signal(30);
  readonly stats = signal<WarehouseFlowStats | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly showTable = signal(false);

  readonly hasMovements = computed(() => (this.stats()?.movementCount ?? 0) > 0);

  // Inbound blue and outbound amber are validated for colour-blind separation; the net line uses neutral ink.
  private static readonly inboundColor = '#1f5fbf';
  private static readonly outboundColor = '#d97706';
  private static readonly netColor = '#0f172a';

  readonly series = computed<ApexAxisChartSeries>(() => {
    const daily = this.stats()?.daily ?? [];
    const point = (value: number, date: string) => ({
      x: Date.parse(`${date}T00:00:00Z`),
      y: value,
    });
    return [
      { name: 'Przyjęcia', type: 'line', data: daily.map((day) => point(day.inbound, day.date)) },
      { name: 'Wydania', type: 'line', data: daily.map((day) => point(day.outbound, day.date)) },
      {
        name: 'Saldo dnia',
        type: 'line',
        data: daily.map((day) => point(day.inbound - day.outbound, day.date)),
      },
    ];
  });

  readonly chart: ApexChart = {
    type: 'line',
    height: 340,
    fontFamily: 'inherit',
    defaultLocale: 'pl',
    locales: [
      {
        name: 'pl',
        options: {
          months: [
            'Styczeń',
            'Luty',
            'Marzec',
            'Kwiecień',
            'Maj',
            'Czerwiec',
            'Lipiec',
            'Sierpień',
            'Wrzesień',
            'Październik',
            'Listopad',
            'Grudzień',
          ],
          shortMonths: [
            'sty',
            'lut',
            'mar',
            'kwi',
            'maj',
            'cze',
            'lip',
            'sie',
            'wrz',
            'paź',
            'lis',
            'gru',
          ],
          days: ['Niedziela', 'Poniedziałek', 'Wtorek', 'Środa', 'Czwartek', 'Piątek', 'Sobota'],
          shortDays: ['ndz', 'pon', 'wt', 'śr', 'czw', 'pt', 'sob'],
          toolbar: {
            download: 'Pobierz',
            exportToSVG: 'Pobierz SVG',
            exportToPNG: 'Pobierz PNG',
            exportToCSV: 'Pobierz CSV',
          },
        },
      },
    ],
    toolbar: {
      show: true,
      tools: {
        download: true,
        selection: false,
        zoom: false,
        zoomin: false,
        zoomout: false,
        pan: false,
        reset: false,
      },
    },
    zoom: { enabled: false },
    animations: { enabled: true, speed: 350 },
    foreColor: '#64748b',
  };
  readonly colors = [
    WarehouseFlowStatsComponent.inboundColor,
    WarehouseFlowStatsComponent.outboundColor,
    WarehouseFlowStatsComponent.netColor,
  ];
  readonly stroke: ApexStroke = {
    width: [3, 3, 2],
    curve: 'smooth',
    lineCap: 'round',
    dashArray: [0, 0, 5],
  };
  readonly markers: ApexMarkers = {
    size: 4,
    strokeColors: '#ffffff',
    strokeWidth: 2,
    hover: { size: 7 },
  };
  readonly dataLabels: ApexDataLabels = { enabled: false };
  readonly grid: ApexGrid = {
    borderColor: '#e2e8f0',
    strokeDashArray: 0,
    xaxis: { lines: { show: false } },
    yaxis: { lines: { show: true } },
    padding: { left: 8, right: 8 },
  };
  readonly xaxis: ApexXAxis = {
    type: 'datetime',
    labels: { format: 'dd.MM', datetimeUTC: true, style: { colors: '#64748b' } },
    axisBorder: { color: '#cbd5e1' },
    axisTicks: { show: false },
    tooltip: { enabled: false },
  };
  readonly yaxis: ApexYAxis = {
    forceNiceScale: true,
    labels: { formatter: (value: number) => `${Math.round(value)}`, style: { colors: '#64748b' } },
    title: { text: 'Sztuki', style: { color: '#64748b', fontWeight: 500 } },
  };
  readonly legend: ApexLegend = {
    position: 'top',
    horizontalAlign: 'left',
    fontSize: '13px',
    markers: { size: 6, shape: 'square', offsetX: -2 },
    itemMargin: { horizontal: 10 },
  };
  readonly tooltip: ApexTooltip = {
    shared: true,
    intersect: false,
    x: { format: 'dddd, dd.MM.yyyy' },
    y: {
      formatter: (value: number) =>
        new Intl.NumberFormat('pl-PL', { maximumFractionDigits: 3 }).format(value),
    },
  };

  readonly productSeries = computed<ApexAxisChartSeries>(() => {
    const products = this.stats()?.topProducts ?? [];
    return [
      { name: 'Przyjęcia', data: products.map((product) => product.inbound) },
      { name: 'Wydania', data: products.map((product) => product.outbound) },
    ];
  });
  readonly productXaxis = computed<ApexXAxis>(() => ({
    categories: (this.stats()?.topProducts ?? []).map((product) => product.productName),
    labels: {
      formatter: (value: string) => `${Math.round(Number(value))}`,
      style: { colors: '#64748b' },
    },
    axisBorder: { show: false },
    axisTicks: { show: false },
  }));

  readonly productChart: ApexChart = {
    type: 'bar',
    height: 300,
    stacked: true,
    fontFamily: 'inherit',
    foreColor: '#64748b',
    toolbar: { show: false },
    animations: { enabled: true, speed: 350 },
  };
  readonly productColors = [
    WarehouseFlowStatsComponent.inboundColor,
    WarehouseFlowStatsComponent.outboundColor,
  ];
  readonly productPlotOptions: ApexPlotOptions = {
    bar: {
      horizontal: true,
      barHeight: '56%',
      borderRadius: 4,
      borderRadiusApplication: 'end',
      borderRadiusWhenStacked: 'last',
    },
  };
  readonly productStroke: ApexStroke = { width: 2, colors: ['#ffffff'] };
  readonly productGrid: ApexGrid = {
    borderColor: '#e2e8f0',
    xaxis: { lines: { show: true } },
    yaxis: { lines: { show: false } },
    padding: { left: 4, right: 12 },
  };
  readonly productYaxis: ApexYAxis = {
    labels: { maxWidth: 170, style: { colors: '#0f172a', fontSize: '13px' } },
  };
  readonly productTooltip: ApexTooltip = {
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

  hours(value: number | null): string {
    if (value === null) return '—';
    return value < 1 ? `${Math.round(value * 60)} min` : `${value.toFixed(1)} h`;
  }

  private load(days: number): void {
    this.loading.set(true);
    this.error.set('');
    this.service
      .getFlow(days)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (stats) => this.stats.set(stats),
        error: (error: HttpErrorResponse) =>
          this.error.set(error.error?.message ?? 'Nie udało się pobrać statystyk przepływu.'),
      });
  }
}
