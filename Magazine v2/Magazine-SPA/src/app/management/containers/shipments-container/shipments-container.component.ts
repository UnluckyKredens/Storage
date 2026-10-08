import { Component, inject, OnInit } from '@angular/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';
import { NotificationService } from '../../../shared/services/notification.service';
import { SelectOption, ShipmentPageData } from '../../models/management.models';
import { ShipmentModalComponent } from '../../modals/shipment/shipment-modal.component';
import { ShipmentService } from '../../services/shipment.service';

@Component({
  selector: 'app-shipments-container',
  imports: [MatBadgeModule, MatIconModule, RouterLink],
  templateUrl: './shipments-container.component.html',
  styleUrl: './shipments-container.component.scss',
})
export class ShipmentsContainerComponent implements OnInit {
  private readonly account = inject(AccountService);
  private readonly service = inject(ShipmentService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  pendingCount = 0;
  pageData: ShipmentPageData | null = null;
  openingMode: 'create' | 'request' | null = null;

  readonly sections = [
    {
      path: 'all',
      label: 'Wszystkie wysyłki',
      description: 'Rejestr ruchu między magazynami',
      icon: 'format_list_bulleted',
      permission: 'shipments.read',
    },
    {
      path: 'pending',
      label: 'Oczekujące',
      description: 'Decyzje kierownika i kontrola dostępności',
      icon: 'pending_actions',
      permission: 'shipments.read',
      badge: true,
    },
    {
      path: 'ready',
      label: 'Gotowe do wysyłki',
      description: 'WZ, etykiety i kompletacja',
      icon: 'task_alt',
      permission: 'shipments.read',
    },
  ];

  ngOnInit(): void {
    this.account.load().subscribe();
    this.account.loadPermissions().subscribe();
    this.service.getPageData().subscribe({
      next: (data) => {
        this.pageData = data;
      },
      error: () => {
        this.pageData = null;
      },
    });
    this.service.getAll().subscribe({
      next: (shipments) => {
        this.pendingCount = shipments.filter(
          (shipment) => shipment.status === 'Oczekuje na akceptację',
        ).length;
      },
    });
  }

  canSee(permission: string): boolean {
    return (
      this.account.user()?.roleId === administratorRoleId ||
      this.account.permissionCodes().includes(permission)
    );
  }

  canCreate(): boolean {
    return this.canSee('shipments.create');
  }

  add(): void {
    if (this.openingMode) return;
    this.openModal('create', 'Wysyłka została utworzona.');
  }

  requestShipment(): void {
    if (this.openingMode) return;
    this.openModal('request', 'Prośba o wysyłkę została wysłana.');
  }

  private get destinationOptions(): SelectOption[] {
    return (
      this.pageData?.destinationWarehouses.map((warehouse) => ({
        value: warehouse.id,
        label: warehouse.name,
      })) ?? []
    );
  }

  private openModal(mode: 'create' | 'request', successMessage: string): void {
    if (!this.canCreate()) {
      this.notification.error('Brak uprawnień do tworzenia wysyłek.');
      return;
    }
    if (!this.pageData) {
      this.openingMode = mode;
      this.service.getPageData().subscribe({
        next: (data) => {
          this.pageData = data;
          this.openingMode = null;
          this.openModal(mode, successMessage);
        },
        error: () => {
          this.openingMode = null;
          this.notification.error('Nie udało się pobrać danych do wysyłki.');
        },
      });
      return;
    }

    this.dialog
      .open(ShipmentModalComponent, {
        width: '820px',
        maxWidth: '96vw',
        data: {
          sourceWarehouseName: this.pageData.sourceWarehouse.name,
          destinations: this.destinationOptions,
          availableProducts: this.pageData.availableProducts,
          mode,
        },
      })
      .afterClosed()
      .subscribe((created) => {
        if (!created) return;
        this.notification.success(successMessage);
        this.refreshPendingCount();
      });
  }

  private refreshPendingCount(): void {
    this.service.getAll().subscribe({
      next: (shipments) => {
        this.pendingCount = shipments.filter(
          (shipment) => shipment.status === 'Oczekuje na akceptację',
        ).length;
      },
    });
  }
}
