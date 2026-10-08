import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { authGuard } from './auth.guard';
import { AccountService } from '../services/account.service';

function token(exp: number): string {
  return `header.${btoa(JSON.stringify({ exp }))}.signature`;
}

describe('authGuard', () => {
  beforeEach(() => {
    localStorage.removeItem('token');
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient()] });
  });

  afterEach(() => localStorage.removeItem('token'));

  function check(url: string) {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );
  }

  it('allows access with a token that has not expired', () => {
    localStorage.setItem('token', token(Math.floor(Date.now() / 1000) + 60));
    expect(check('/main/management')).toBe(true);
  });

  it('redirects an expired token to login and saves the requested URL', () => {
    localStorage.setItem('token', token(Math.floor(Date.now() / 1000) - 60));
    TestBed.inject(AccountService).user.set({
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
    const result = check('/main/management/products');
    expect(TestBed.inject(Router).serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe(
      '/auth?returnUrl=%2Fmain%2Fmanagement%2Fproducts',
    );
    expect(localStorage.getItem('token')).toBeNull();
    expect(TestBed.inject(AccountService).user()).toBeNull();
  });

  it('rejects a malformed token', () => {
    localStorage.setItem('token', 'bad-token');
    expect(check('/main/management')).not.toBe(true);
    expect(localStorage.getItem('token')).toBeNull();
  });
});
