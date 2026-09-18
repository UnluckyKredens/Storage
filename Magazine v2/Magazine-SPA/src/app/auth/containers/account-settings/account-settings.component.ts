import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { AccountService, AccountUser } from '../../services/account.service';

@Component({
  selector: 'app-account-settings',
  imports: [FormInputComponent, MatButtonModule, ReactiveFormsModule],
  templateUrl: './account-settings.component.html',
  styleUrl: './account-settings.component.scss',
})
export class AccountSettingsComponent implements OnInit {
  readonly account = inject(AccountService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notification = inject(NotificationService);
  readonly form = this.fb.nonNullable.group({
    login: ['', [Validators.required, Validators.maxLength(100)]],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
    currentPassword: [''],
    newPassword: ['', [Validators.minLength(8), Validators.maxLength(128)]],
    confirmPassword: [''],
  });

  loading = true;
  saving = false;
  error = '';
  success = '';

  ngOnInit(): void {
    this.load();
    this.account
      .loadPermissions()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: () => {
          this.notification.error('Nie udało się pobrać uprawnień.');
          this.cdr.markForCheck();
        },
      });
  }

  load(): void {
    this.loading = true;
    this.account
      .load()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.fillForm(user);
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          this.notification.error(this.errorText(error));
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  save(): void {
    this.error = '';
    this.success = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.showError('Popraw dane formularza.');
      return;
    }

    const values = this.form.getRawValue();
    if (values.newPassword && values.newPassword !== values.confirmPassword) {
      this.showError('Nowe hasła muszą być takie same.');
      return;
    }
    if (values.newPassword && !values.currentPassword) {
      this.showError('Podaj obecne hasło.');
      return;
    }

    this.saving = true;
    this.account
      .update({
        login: values.login,
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        currentPassword: values.newPassword ? values.currentPassword : null,
        newPassword: values.newPassword || null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.fillForm(result.user);
          this.form.patchValue({ currentPassword: '', newPassword: '', confirmPassword: '' });
          this.saving = false;
          this.success = 'Ustawienia konta zostały zapisane.';
          this.notification.success(this.success);
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          this.saving = false;
          this.showError(this.errorText(error));
          this.cdr.markForCheck();
        },
      });
  }

  private fillForm(user: AccountUser): void {
    this.form.patchValue({
      login: user.login,
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
    });
  }

  private errorText(error: HttpErrorResponse): string {
    console.error(error);
    return error.error?.message ?? 'Nie udało się wykonać operacji.';
  }

  private showError(message: string): void {
    this.error = message;
    this.notification.error(message);
  }
}
