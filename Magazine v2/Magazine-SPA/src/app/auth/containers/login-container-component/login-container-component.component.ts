import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { AccountService } from '../../services/account.service';
import { authStore } from '../../store/auth.store';
import { hasValidToken } from '../../token';

@Component({
  selector: 'app-login-container-component',
  standalone: true,
  imports: [FormInputComponent, MatButtonModule, ReactiveFormsModule],
  templateUrl: './login-container-component.component.html',
  styleUrl: './login-container-component.component.scss',
})
export class LoginContainerComponentComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authStore = inject(authStore);
  private readonly account = inject(AccountService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackbar = inject(MatSnackBar);

  readonly loginForm = this.fb.group({
    login: new FormControl('', Validators.required),
    password: new FormControl('', Validators.required),
  });

  ngOnInit(): void {
    if (hasValidToken(localStorage.getItem('token'))) {
      void this.router.navigateByUrl(this.destination());
    } else {
      this.account.logout();
    }
  }

  login(): void {
    if (this.loginForm.valid) {
      const { login, password } = this.loginForm.value;
      this.authStore.authUser(login!, password!, () => {
        void this.router.navigateByUrl(this.destination());
      });
    } else {
      this.snackbar.open('Wypełnij wszystkie pola', 'Zamknij', {
        duration: 3000,
      });
    }
  }

  private destination(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl?.startsWith('/main/') ? returnUrl : '/main/management';
  }
}
