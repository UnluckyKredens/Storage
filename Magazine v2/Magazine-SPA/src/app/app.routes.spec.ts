import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { routes } from './app.routes';

function token(exp: number): string {
  return `header.${btoa(JSON.stringify({ exp }))}.signature`;
}

describe('app routes', () => {
  let router: Router;

  beforeEach(() => {
    localStorage.removeItem('token');
    TestBed.configureTestingModule({ providers: [provideRouter(routes), provideHttpClient()] });
    router = TestBed.inject(Router);
  });

  afterEach(() => localStorage.removeItem('token'));

  it('opens management for an authenticated user entering /main', async () => {
    localStorage.setItem('token', token(Math.floor(Date.now() / 1000) + 60));
    await router.navigateByUrl('/main');
    expect(router.url).toBe('/main/management');
  });

  it('checks the token again when moving between protected pages', async () => {
    localStorage.setItem('token', token(Math.floor(Date.now() / 1000) + 60));
    await router.navigateByUrl('/main/management');
    localStorage.setItem('token', token(Math.floor(Date.now() / 1000) - 60));
    await router.navigateByUrl('/main/management/products');
    expect(router.url).toBe('/auth?returnUrl=%2Fmain%2Fmanagement%2Fproducts');
  });

  it('protects account settings', async () => {
    await router.navigateByUrl('/main/account');
    expect(router.url).toBe('/auth?returnUrl=%2Fmain%2Faccount');
  });

  it('opens the shipments module for an authenticated user', async () => {
    localStorage.setItem('token', token(Math.floor(Date.now() / 1000) + 60));
    await router.navigateByUrl('/main/shipments');
    expect(router.url).toBe('/main/shipments');
  });
});
