import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import localePl from '@angular/common/locales/pl';
import {
  ApplicationConfig,
  DEFAULT_CURRENCY_CODE,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/interceptors/auth.interceptor';
import { UnauthorizedInterceptor } from './core/auth/interceptors/unauthorized.interceptor';
import { PolishPaginatorIntl } from './core/i18n/polish-paginator-intl';

registerLocaleData(localePl);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(withInterceptors([authInterceptor, UnauthorizedInterceptor])),
    provideRouter(routes),
    { provide: LOCALE_ID, useValue: 'pl' },
    { provide: DEFAULT_CURRENCY_CODE, useValue: 'PLN' },
    { provide: MatPaginatorIntl, useClass: PolishPaginatorIntl },
  ],
};
