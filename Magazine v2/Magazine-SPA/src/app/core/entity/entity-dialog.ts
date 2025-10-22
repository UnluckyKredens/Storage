import { HttpErrorResponse } from '@angular/common/http';
import { Directive, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { NotificationService } from '../../shared/services/notification.service';

export interface EntityDialogData<T> {
  mode: 'details' | 'form' | 'delete';
  row: T | null;
}

@Directive()
export abstract class EntityDialog<T> {
  readonly data = inject<EntityDialogData<T>>(MAT_DIALOG_DATA);
  protected readonly dialogRef = inject(MatDialogRef<unknown, boolean>);
  protected readonly notification = inject(NotificationService);

  abstract form: object;
  protected abstract readonly titles: {
    details: string;
    delete: string;
    edit: string;
    add: string;
  };
  protected abstract request(): Observable<unknown>;

  saving = false;

  get title(): string {
    if (this.data.mode === 'details') return this.titles.details;
    if (this.data.mode === 'delete') return this.titles.delete;
    return this.data.row ? this.titles.edit : this.titles.add;
  }

  save(): void {
    this.saving = true;
    this.request().subscribe({
      next: () => this.dialogRef.close(true),
      error: (error: HttpErrorResponse) => {
        this.saving = false;
        this.notification.error(error.error?.message ?? 'Nie udało się zapisać danych.');
      },
    });
  }

  display(value: unknown): string {
    if (value === null || value === undefined || value === '') return '—';
    if (typeof value === 'boolean') return value ? 'Tak' : 'Nie';
    if (Array.isArray(value)) return value.join(', ') || '—';
    return String(value);
  }
}
