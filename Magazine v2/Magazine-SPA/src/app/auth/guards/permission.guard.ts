import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, forkJoin, map, of } from 'rxjs';
import { administratorRoleId } from '../roles';
import { AccountService } from '../services/account.service';

export const permissionGuard: CanActivateFn = (route) => {
  const account = inject(AccountService);
  const router = inject(Router);
  const permission = route.data['permission'] as string;

  return forkJoin([account.load(), account.loadPermissions()]).pipe(
    map(([user, permissions]) =>
      user.roleId === administratorRoleId || permissions.includes(permission)
        ? true
        : router.createUrlTree(['/main/shipments']),
    ),
    catchError(() => of(router.createUrlTree(['/main/shipments']))),
  );
};
