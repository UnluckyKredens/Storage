import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Category } from '../../models/management.models';
import { CategoryService } from '../../services/category.service';

interface CategoryModalData {
  mode: 'details' | 'form' | 'delete';
  row: Category | null;
}

@Component({
  selector: 'app-category-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormTextareaComponent,
  ],
  templateUrl: './category-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class CategoryModalComponent {
  readonly data = inject<CategoryModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(CategoryService);
  private readonly dialogRef = inject(MatDialogRef<CategoryModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    name: String(this.data.row?.name ?? ''),
    description: String(this.data.row?.description ?? ''),
  };
  saving = false;

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły kategorii';
    if (this.data.mode === 'delete') return 'Usuń kategorię';
    return this.data.row ? 'Edytuj kategorię' : 'Dodaj kategorię';
  }

  save(): void {
    this.saving = true;

    this.service.save(this.data.row?.id ?? null, this.form).subscribe({
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
