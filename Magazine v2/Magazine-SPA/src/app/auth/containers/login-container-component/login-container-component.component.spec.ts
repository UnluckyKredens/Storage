import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';

import { LoginContainerComponentComponent } from './login-container-component.component';
import { authStore } from '../../store/auth.store';
import { AccountService } from '../../services/account.service';

describe('LoginContainerComponentComponent', () => {
  let component: LoginContainerComponentComponent;
  let fixture: ComponentFixture<LoginContainerComponentComponent>;
  const authUser = vi.fn((_login: string, _password: string, success: () => void) => success());

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginContainerComponentComponent],
      providers: [
        provideRouter([]),
        { provide: authStore, useValue: { authUser } },
        { provide: AccountService, useValue: { logout: vi.fn() } },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({ returnUrl: '/main/management/products' }),
            },
          },
        },
      ],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginContainerComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('goes back to the requested page after login', () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    component.loginForm.setValue({ login: 'user', password: 'password' });
    component.login();
    expect(authUser).toHaveBeenCalledWith('user', 'password', expect.any(Function));
    expect(navigate).toHaveBeenCalledWith('/main/management/products');
  });
});
