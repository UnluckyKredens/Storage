import { Component, computed, inject, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';

@Component({
  selector: 'app-management-container',
  templateUrl: './management-container.component.html',
  styleUrls: ['./management-container.component.scss'],
  imports: [MatIconModule, RouterLink],
})

export class ManagementContainerComponent implements OnInit {
  private readonly account = inject(AccountService);

  readonly sections = [
    {
      path: 'products',
      label: 'Produkty',
      icon: 'memory',
      permission: 'products.read',
      category: 'Magazyn',
    },
    {
      path: 'categories',
      label: 'Kategorie',
      icon: 'category',
      permission: 'dictionaries.manage',
      category: 'Ustawienia',
    },
    {
      path: 'units',
      label: 'Jednostki miary',
      icon: 'straighten',
      permission: 'dictionaries.manage',
      category: 'Ustawienia',
    },
    {
      path: 'warehouses',
      label: 'Magazyny',
      icon: 'warehouse',
      permission: 'warehouses.read',
      category: 'Magazyn',
    },
    {
      path: 'locations',
      label: 'Lokalizacje',
      icon: 'pin_drop',
      permission: 'warehouses.read',
      category: 'Magazyn',
    },
    {
      path: '/main/inventory',
      label: 'Stany magazynowe',
      icon: 'inventory_2',
      permission: 'inventory.read',
      category: 'Magazyn',
    },
    {
      path: '/main/inventory/operations',
      label: 'Dokumenty PW/RW/MM',
      icon: 'assignment',
      permission: 'inventory.read',
      category: 'Magazyn',
    },
    {
      path: '/main/inventory/movements',
      label: 'Ruchy magazynowe',
      icon: 'sync_alt',
      permission: 'inventory.read',
      category: 'Magazyn',
    },
    {
      path: '/main/shipments',
      label: 'Wysyłki',
      icon: 'local_shipping',
      permission: 'shipments.read',
      category: 'Magazyn',
    },
    {
      path: '/main/purchase-orders',
      label: 'Zamówienia zewnętrzne',
      icon: 'receipt_long',
      permission: 'purchase-orders.read',
      category: 'Magazyn',
    },
    {
      path: '/main/storefront/packing',
      label: 'Zakupy do spakowania',
      icon: 'inventory',
      permission: 'shipments.read',
      category: 'Magazyn',
    },
    {
      path: '/main/storefront/orders',
      label: 'Zamówienia sklepu',
      icon: 'local_shipping',
      permission: 'shipments.read',
      category: 'Magazyn',
    },
    {
      path: 'contractors',
      label: 'Kontrahenci',
      icon: 'business',
      permission: 'contractors.read',
      category: 'Ustawienia',
    },
    {
      path: 'users',
      label: 'Użytkownicy',
      icon: 'groups',
      permission: 'users.read',
      category: 'Ustawienia',
    },
    {
      path: 'roles',
      label: 'Role',
      icon: 'admin_panel_settings',
      permission: 'roles.manage',
      category: 'Ustawienia',
    },
    {
      path: 'permissions',
      label: 'Uprawnienia',
      icon: 'key',
      permission: 'roles.manage',
      category: 'Ustawienia',
    },
    {
      path: 'audit-logs',
      label: 'Audit log',
      icon: 'manage_search',
      permission: 'roles.manage',
      category: 'Ustawienia',
    },
  ];

  readonly visibleSections = computed(() => this.getVisibleSections());

  ngOnInit(): void {
    this.account.load().subscribe();
    this.account.loadPermissions().subscribe();
  }

  getVisibleSections() {
    const filteredSections = this.sections.filter(
      (section) =>
        this.account.user()?.roleId === administratorRoleId ||
        this.account.permissionCodes().includes(section.permission),
    );

    const uniqueCategories = [...new Set(filteredSections.map((s) => s.category))];

    const result = uniqueCategories.map((category) => ({
      category,
      sections: filteredSections
        .filter((s) => s.category === category)
        .map((s) => ({
          path: s.path,
          label: s.label,
          icon: s.icon,
        })),
    }));

    return result;
  }
}
