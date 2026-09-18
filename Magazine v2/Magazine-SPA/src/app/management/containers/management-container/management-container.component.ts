import { Component, computed, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { administratorRoleId } from '../../../auth/roles';
import { AccountService } from '../../../auth/services/account.service';

@Component({
  selector: 'app-management-container',
  templateUrl: './management-container.component.html',
  styleUrls: ['./management-container.component.scss'],
  imports: [RouterLink],
})
export class ManagementContainerComponent implements OnInit {
  private readonly account = inject(AccountService);

  readonly sections = [
    { path: 'products', label: 'Produkty', permission: 'products.read', category: 'Magazyn' },
    {
      path: 'categories',
      label: 'Kategorie',
      permission: 'dictionaries.manage',
      category: 'Słowniki',
    },
    {
      path: 'units',
      label: 'Jednostki miary',
      permission: 'dictionaries.manage',
      category: 'Słowniki',
    },
    {
      path: 'warehouses',
      label: 'Magazyny',
      permission: 'warehouses.read',
      category: 'Magazyn',
    },
    {
      path: 'locations',
      label: 'Lokalizacje',
      permission: 'warehouses.read',
      category: 'Magazyn',
    },
    {
      path: 'inventory',
      label: 'Stany magazynowe',
      permission: 'inventory.read',
      category: 'Magazyn',
    },
    {
      path: 'contractors',
      label: 'Kontrahenci',
      permission: 'contractors.read',
      category: 'Osoby',
    },
    { path: 'users', label: 'Użytkownicy', permission: 'users.read', category: 'Osoby' },
    { path: 'roles', label: 'Role', permission: 'roles.manage', category: 'Osoby' },
    { path: 'permissions', label: 'Uprawnienia', permission: 'roles.manage', category: 'Osoby' },
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
        .map((s) => ({ path: s.path, label: s.label })),
    }));

    return result;
  }
}
