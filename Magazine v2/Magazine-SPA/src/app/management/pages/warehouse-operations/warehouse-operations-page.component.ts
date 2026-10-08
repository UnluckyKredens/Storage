import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { forkJoin } from 'rxjs';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { SearchSelectComponent } from '../../../shared/components/search-select/search-select.component';
import { TableNavigationComponent } from '../../../shared/components/table-navigation-component/table-navigation-component.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { SelectOption, WarehouseOperation } from '../../models/management.models';
import { InventoryService } from '../../services/inventory.service';
import {
  WarehouseOperationFormItem,
  WarehouseOperationService,
} from '../../services/warehouse-operation.service';

interface OperationTypeOption {
  value: number;
  label: string;
}

@Component({
  selector: 'app-warehouse-operations-page',
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    FormInputComponent,
    FormTextareaComponent,
    SearchSelectComponent,
    TableNavigationComponent,
  ],
  templateUrl: './warehouse-operations-page.component.html',
  styleUrls: ['../management-page.scss', './warehouse-operations-page.component.scss'],
})
export class WarehouseOperationsPageComponent implements OnInit {
  private readonly service = inject(WarehouseOperationService);
  private readonly inventoryService = inject(InventoryService);
  private readonly account = inject(AccountService);
  private readonly notification = inject(NotificationService);

  readonly displayedColumns = ['number', 'type', 'warehouseName', 'completedOnUtc', 'items'];
  readonly operationTypes: OperationTypeOption[] = [
    { value: 1, label: 'PW' },
    { value: 2, label: 'RW' },
    { value: 3, label: 'MM' },
    { value: 4, label: 'Korekta' },
    { value: 5, label: 'Inwentaryzacja' },
  ];

  readonly operations = signal<WarehouseOperation[]>([]);
  readonly products = signal<SelectOption[]>([]);
  readonly locations = signal<SelectOption[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  type = 1;
  notes = '';
  items: WarehouseOperationFormItem[] = [this.emptyItem()];
  private initialized = false;

  constructor() {
    effect(() => {
      this.account.activeWarehouseId();
      if (this.initialized) this.load();
    });
  }

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin([this.account.load(), this.account.loadPermissions()]).subscribe({
      next: () => {
        this.initialized = true;
        this.load();
        if (this.canManage) this.loadOptions();
      },
      error: (error: HttpErrorResponse) => {
        this.initialized = true;
        this.loading.set(false);
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać danych konta.');
      },
    });
  }

  get canManage(): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes('inventory.manage')
    );
  }

  get usesSource(): boolean {
    return this.type !== 1;
  }

  get usesDestination(): boolean {
    return this.type === 1 || this.type === 3;
  }

  get usesTargetQuantity(): boolean {
    return this.type === 4 || this.type === 5;
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (rows) => {
        this.operations.set(rows);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.notification.error(error.error?.message ?? 'Nie udało się pobrać dokumentów.');
      },
    });
  }

  loadOptions(): void {
    this.inventoryService.getFormOptions().subscribe({
      next: (options) => {
        this.products.set(options.products);
        this.locations.set(options.locations);
      },
      error: () => this.notification.error('Nie udało się pobrać produktów i lokalizacji.'),
    });
  }

  addItem(): void {
    this.items = [...this.items, this.emptyItem()];
  }

  removeItem(index: number): void {
    if (this.items.length === 1) return;
    this.items = this.items.filter((_, itemIndex) => itemIndex !== index);
  }

  save(): void {
    if (!this.canManage || this.saving()) return;
    const payloadItems = this.items.map((item) => ({
      productId: item.productId,
      sourceLocationId: this.usesSource ? item.sourceLocationId : null,
      destinationLocationId: this.usesDestination ? item.destinationLocationId : null,
      quantity: this.usesTargetQuantity ? Number(item.quantity || 0) : Number(item.quantity),
      targetQuantity: this.usesTargetQuantity ? Number(item.targetQuantity) : null,
    }));

    this.saving.set(true);
    this.service
      .complete({
        type: this.type,
        warehouseId: null,
        notes: this.notes.trim() || null,
        items: payloadItems,
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.notification.success('Dokument magazynowy zaksięgowany.');
          this.notes = '';
          this.items = [this.emptyItem()];
          this.load();
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.notification.error(error.error?.message ?? 'Nie udało się zaksięgować dokumentu.');
        },
      });
  }

  operationTypeLabel(type: string): string {
    return (
      {
        InternalReceipt: 'PW',
        InternalIssue: 'RW',
        InternalTransfer: 'MM',
        Correction: 'Korekta',
        InventoryCount: 'Inwentaryzacja',
      }[type] ?? type
    );
  }

  private emptyItem(): WarehouseOperationFormItem {
    return {
      productId: '',
      sourceLocationId: null,
      destinationLocationId: null,
      quantity: 1,
      targetQuantity: null,
    };
  }
}
