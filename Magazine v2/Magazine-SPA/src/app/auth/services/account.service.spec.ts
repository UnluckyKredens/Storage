import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { AccountService } from './account.service';

describe('AccountService', () => {
  beforeEach(() => TestBed.configureTestingModule({ providers: [provideHttpClient()] }));
  afterEach(() => localStorage.removeItem('token'));

  it('clears the account and token when logging out', () => {
    const account = TestBed.inject(AccountService);
    localStorage.setItem('token', 'saved-token');
    account.user.set({
      id: 'user-id',
      login: 'anna',
      firstName: 'Anna',
      lastName: 'Kowalska',
      email: 'anna@example.com',
      roleId: 'role-id',
      roleName: 'Pracownik',
      warehouseId: 'warehouse-id',
      warehouseName: 'Magazyn Warszawa',
    });
    account.permissionCodes.set(['products.read']);

    account.logout();

    expect(localStorage.getItem('token')).toBeNull();
    expect(account.user()).toBeNull();
    expect(account.permissionCodes()).toEqual([]);
  });
});
