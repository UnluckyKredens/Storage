import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { AccountService, AccountUser } from '../../services/account.service';
import { AccountSettingsComponent } from './account-settings.component';

const base = environment.apiUrl;
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

describe('AccountSettingsComponent', () => {
  let fixture: ComponentFixture<AccountSettingsComponent>;
  let component: AccountSettingsComponent;
  let http: HttpTestingController;

  beforeEach(async () => {
    localStorage.removeItem('token');
    await TestBed.configureTestingModule({
      imports: [AccountSettingsComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountSettingsComponent);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne(`${base}/Auth/me`).flush(user);
    http.expectOne(`${base}/Auth/me/permissions`).flush({ permissionCodes: ['products.read'] });
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
    localStorage.removeItem('token');
  });

  it('shows account data and permissions', () => {
    expect(fixture.nativeElement.textContent).toContain('Anna Kowalska');
    expect(fixture.nativeElement.textContent).toContain('products.read');
    expect(component.form.controls.email.value).toBe('anna@example.com');
  });

  it('saves changes and replaces the token used by the session', () => {
    component.form.patchValue({ firstName: 'Maria' });
    component.save();
    const request = http.expectOne(`${base}/Auth/me`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      login: 'anna',
      firstName: 'Maria',
      lastName: 'Kowalska',
      email: 'anna@example.com',
      currentPassword: null,
      newPassword: null,
    });
    request.flush({ token: 'new-token', user: { ...user, firstName: 'Maria' } });
    fixture.detectChanges();
    expect(localStorage.getItem('token')).toBe('new-token');
    expect(TestBed.inject(AccountService).user()?.firstName).toBe('Maria');
    expect(fixture.nativeElement.textContent).toContain('Ustawienia konta zostały zapisane.');
  });

  it('requires the current password before changing the password', () => {
    component.form.patchValue({ newPassword: 'new-password', confirmPassword: 'new-password' });
    component.save();
    expect(component.error).toBe('Podaj obecne hasło.');
    http.expectNone(`${base}/Auth/me`);
  });

  it('sends the current password when changing the password', () => {
    component.form.patchValue({
      currentPassword: 'old-password',
      newPassword: 'new-password',
      confirmPassword: 'new-password',
    });
    component.save();
    const request = http.expectOne(`${base}/Auth/me`);
    expect(request.request.body.currentPassword).toBe('old-password');
    expect(request.request.body.newPassword).toBe('new-password');
    request.flush(
      { message: 'Nieprawidłowe obecne hasło.' },
      { status: 400, statusText: 'Bad Request' },
    );
    fixture.detectChanges();
    expect(component.error).toBe('Nieprawidłowe obecne hasło.');
    expect(component.form.controls.newPassword.value).toBe('new-password');
  });
});
