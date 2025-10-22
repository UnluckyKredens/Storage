import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../services/account.service';
import { hasValidToken } from '../token';

export const authGuard: CanActivateFn = (_route, state) => {
  const router = inject(Router);
  const account = inject(AccountService);

  if (hasValidToken(localStorage.getItem('token'))) {
    return true;
  }

  account.logout();
  return router.createUrlTree(['/auth'], {
    queryParams: { returnUrl: state.url },
  });
};
