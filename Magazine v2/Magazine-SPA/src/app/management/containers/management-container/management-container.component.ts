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
      description: 'Kartoteka sprzętu, SKU, ceny i kody kreskowe',
      icon: 'memory',
      permission: 'products.read',
      category: 'Magazyn',
    },
    {
      path: 'categories',
      label: 'Kategorie',
      description: 'Grupy asortymentu i porządek katalogu',
      icon: 'category',
      permission: 'dictionaries.manage',
      category: 'Ustawienia',
    },
    {
      path: 'units',
      label: 'Jednostki miary',
      description: 'Sztuki, komplety, opakowania i paczki',
      icon: 'straighten',
      permission: 'dictionaries.manage',
      category: 'Ustawienia',
    },
    {
      path: 'warehouses',
      label: 'Magazyny',
      description: 'Oddziały, zaplecza i magazyny docelowe',
      icon: 'warehouse',
      permission: 'warehouses.read',
      category: 'Magazyn',
    },
    {
      path: 'locations',
      label: 'Lokalizacje',
      description: 'Strefy, regały i miejsca odkładcze',
      icon: 'pin_drop',
      permission: 'warehouses.read',
      category: 'Magazyn',
    },
    {
      path: 'inventory',
      label: 'Stany magazynowe',
      description: 'Ilości dostępne, rezerwacje i miejsca składowania',
      icon: 'inventory_2',
      permission: 'inventory.read',
      category: 'Magazyn',
    },
    {
      path: 'warehouse-operations',
      label: 'Dokumenty PW/RW/MM',
      description: 'Przyjęcia, rozchody, przesunięcia i korekty',
      icon: 'assignment',
      permission: 'inventory.read',
      category: 'Magazyn',
    },
    {
      path: 'stock-movements',
      label: 'Ruchy magazynowe',
      description: 'Pełna księga zmian ilości i źródeł ruchu',
      icon: 'sync_alt',
      permission: 'inventory.read',
      category: 'Magazyn',
    },
    {
      path: 'shipments',
      label: 'Wysyłki',
      description: 'Ruch towaru między magazynami i akceptacje',
      icon: 'local_shipping',
      permission: 'shipments.read',
      category: 'Magazyn',
    },
    {
      path: 'purchase-orders',
      label: 'Zamówienia zewnętrzne',
      description: 'Dostawcy, faktury, dokumenty i przyjęcia',
      icon: 'receipt_long',
      permission: 'purchase-orders.read',
      category: 'Magazyn',
    },
    {
      path: 'storefront-packing',
      label: 'Zakupy do spakowania',
      description: 'Zamówienia ze sklepu, kompletacja i pakowanie',
      icon: 'inventory',
      permission: 'shipments.read',
      category: 'Magazyn',
    },
    {
      path: 'contractors',
      label: 'Kontrahenci',
      description: 'Dostawcy, firmy serwisowe i odbiorcy',
      icon: 'business',
      permission: 'contractors.read',
      category: 'Ustawienia',
    },
    {
      path: 'users',
      label: 'Użytkownicy',
      description: 'Pracownicy, magazyny przypisane do kont',
      icon: 'groups',
      permission: 'users.read',
      category: 'Ustawienia',
    },
    {
      path: 'roles',
      label: 'Role',
      description: 'Zakres odpowiedzialności w systemie',
      icon: 'admin_panel_settings',
      permission: 'roles.manage',
      category: 'Ustawienia',
    },
    {
      path: 'permissions',
      label: 'Uprawnienia',
      description: 'Dostęp do operacji i widoków',
      icon: 'key',
      permission: 'roles.manage',
      category: 'Ustawienia',
    },
    {
      path: 'audit-logs',
      label: 'Audit log',
      description: 'Historia kluczowych zmian w systemie',
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
          description: s.description,
          icon: s.icon,
        })),
    }));

    return result;
  }
}
