import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import {
  AccountService,
  AccountUser,
  WarehouseOption,
} from '../../../auth/services/account.service';

import { NavigationBarComponent } from './navigation-bar.component';

describe('NavigationBarComponent', () => {
  let component: NavigationBarComponent;
  let fixture: ComponentFixture<NavigationBarComponent>;
  const user: AccountUser = {
    id: 'user-id',
    login: 'anna',
    firstName: 'Anna',
    lastName: 'Kowalska',
    email: 'anna@example.com',
    roleId: 'role-id',
    roleName: 'Pracownik',
    warehouseId: 'warehouse-id',
    warehouseName: 'Magazyn Warszawa',
  };
  const account = {
    user: signal<AccountUser | null>(user),
    load: vi.fn(() => of(account.user()!)),
    loadPermissions: vi.fn(() => of(account.permissionCodes())),
    loadWarehouses: vi.fn(() => of([])),
    permissionCodes: signal<string[]>(['stock-documents.receive']),
    warehouses: signal<WarehouseOption[]>([]),
    activeWarehouseId: signal<string | null>('warehouse-id'),
    setActiveWarehouse: vi.fn(),
    logout: vi.fn(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavigationBarComponent],
      providers: [provideRouter([]), { provide: AccountService, useValue: account }],
    }).compileComponents();
  });

  beforeEach(() => {
    vi.clearAllMocks();
    account.user.set(user);
    account.permissionCodes.set(['stock-documents.receive']);
    account.warehouses.set([]);
    fixture = TestBed.createComponent(NavigationBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('links only to existing sections', () => {
    const links = Array.from(fixture.nativeElement.querySelectorAll('a')) as HTMLAnchorElement[];
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      '/main/management/inventory',
      '/main/management',
      '/main/shipments',
    ]);
  });

  it('shows the account name and logs out', () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    expect(fixture.nativeElement.textContent).toContain('Anna Kowalska');
    const menuTrigger = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    menuTrigger.click();
    fixture.detectChanges();
    const menuButtons = Array.from(document.querySelectorAll('[mat-menu-item]')) as HTMLElement[];
    expect(menuButtons.map((item) => item.textContent?.trim())).toEqual([
      'settingsUstawienia konta',
      'logoutWyloguj',
    ]);
    menuButtons[1].click();
    expect(account.logout).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith('/auth');
  });

  it('loads warehouses for an administrator and changes the active warehouse', () => {
    fixture.destroy();
    account.user.set({
      ...user,
      roleId: '10000000-0000-0000-0000-000000000001',
      roleName: 'Administrator',
      warehouseId: null,
      warehouseName: null,
    });
    account.warehouses.set([
      { id: 'warehouse-1', name: 'Warszawa' },
      { id: 'warehouse-2', name: 'Poznań' },
    ]);
    fixture = TestBed.createComponent(NavigationBarComponent);
    fixture.detectChanges();

    expect(account.loadWarehouses).toHaveBeenCalled();
    const select = fixture.nativeElement.querySelector('mat-select') as HTMLElement;
    select.click();
    fixture.detectChanges();
    expect(document.querySelector('input[aria-label="Szukaj magazynu"]')).toBeTruthy();

    const searchInput = document.querySelector(
      'input[aria-label="Szukaj magazynu"]',
    ) as HTMLInputElement;
    searchInput.value = 'poz';
    searchInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const optionsText = Array.from(document.querySelectorAll('mat-option')).map((option) =>
      option.textContent?.trim(),
    );
    expect(optionsText).toContain('Poznań');
    expect(optionsText).not.toContain('Warszawa');

    fixture.componentInstance.selectWarehouse('warehouse-2');
    expect(account.setActiveWarehouse).toHaveBeenCalledWith('warehouse-2');
  });
});
