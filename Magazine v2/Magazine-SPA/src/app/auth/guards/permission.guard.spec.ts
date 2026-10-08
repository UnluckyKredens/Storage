import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { firstValueFrom, Observable, of } from 'rxjs';
import { AccountService, AccountUser } from '../services/account.service';
import { permissionGuard } from './permission.guard';

describe('permissionGuard', () => {
  const user: AccountUser = {
    id: 'user-id',
    login: 'anna',
    firstName: 'Anna',
    lastName: 'Kowalska',
    email: 'anna@example.com',
    roleId: '10000000-0000-0000-0000-000000000003',
    roleName: 'Pracownik',
    warehouseId: 'warehouse-id',
    warehouseName: 'Magazyn Warszawa',
  };
  let permissions: string[];

  beforeEach(() => {
    permissions = ['products.read', 'inventory.read'];
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: AccountService,
          useValue: {
            load: () => of(user),
            loadPermissions: () => of(permissions),
          },
        },
      ],
    });
  });

  async function check(permission: string) {
    const route = { data: { permission } } as unknown as ActivatedRouteSnapshot;
    const result = TestBed.runInInjectionContext(() =>
      permissionGuard(route, {} as RouterStateSnapshot),
    );
    return firstValueFrom(result as Observable<boolean | ReturnType<Router['createUrlTree']>>);
  }

  it('allows a worker to open a permitted section', async () => {
    await expect(check('products.read')).resolves.toBe(true);
  });

  it('redirects a worker trying to open a forbidden section', async () => {
    const result = await check('users.read');
    expect(TestBed.inject(Router).serializeUrl(result as ReturnType<Router['createUrlTree']>)).toBe(
      '/main/management',
    );
  });
});
