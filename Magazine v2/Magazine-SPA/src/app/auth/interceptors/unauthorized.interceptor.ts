import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AccountService } from '../services/account.service';

export const UnauthorizedInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const account = inject(AccountService);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !request.url.endsWith('/Auth/login')) {
        const returnUrl = router.url.startsWith('/main') ? router.url : '/main/management';
        account.logout();
        void router.navigate(['/auth'], { queryParams: { returnUrl } });
      }
      return throwError(() => error);
    }),
  );
};
