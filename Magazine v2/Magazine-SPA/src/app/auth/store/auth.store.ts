import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

interface UserState {
  isLoading: boolean;
  errors: string | null;
}

const initialState: UserState = {
  isLoading: false,
  errors: null,
};

export const authStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),

  withMethods((state, authService = inject(AuthService), snackbar = inject(MatSnackBar)) => ({
    setLoading: (isLoading: boolean) => patchState(state, { isLoading }),
    setErrors: (errors: string | null) => patchState(state, { errors }),

    authUser: (login: string, password: string, success: () => void) => {
      patchState(state, { isLoading: true, errors: null });
      authService.login(login, password).subscribe({
        next: (response) => {
          patchState(state, { isLoading: false });
          localStorage.setItem('token', response.token);
          success();
        },
        error: (error) => {
          const errorMessage = error.error?.message ?? 'Nieprawidłowe dane logowania';
          patchState(state, { isLoading: false, errors: errorMessage });
          snackbar.open(errorMessage, 'Zamknij', {
            duration: 3000,
          });
        },
      });
    },
  })),
);
