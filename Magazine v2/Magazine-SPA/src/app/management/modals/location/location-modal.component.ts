import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { FormInputComponent } from '../../../shared/components/form-input/form-input.component';
import { FormSelectComponent } from '../../../shared/components/form-select/form-select.component';
import { FormTextareaComponent } from '../../../shared/components/form-textarea/form-textarea.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { Location, SelectOption } from '../../models/management.models';
import { LocationService } from '../../services/location.service';

interface LocationModalData {
  mode: 'details' | 'form' | 'delete';
  row: Location | null;
}

@Component({
  selector: 'app-location-modal',
  imports: [
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    DialogHeaderComponent,
    FormInputComponent,
    FormSelectComponent,
    FormTextareaComponent,
  ],
  templateUrl: './location-modal.component.html',
  styleUrl: '../management-modal.scss',
})
export class LocationModalComponent implements OnInit {
  readonly data = inject<LocationModalData>(MAT_DIALOG_DATA);
  private readonly service = inject(LocationService);
  private readonly dialogRef = inject(MatDialogRef<LocationModalComponent, boolean>);
  private readonly notification = inject(NotificationService);

  form = {
    warehouseId: String(this.data.row?.warehouseId ?? ''),
    locationCode: String(this.data.row?.locationCode ?? ''),
    description: String(this.data.row?.description ?? ''),
  };
  saving = false;
  warehouses: SelectOption[] = [];

  ngOnInit(): void {
    if (this.data.mode !== 'form') return;
    this.service.getFormOptions().subscribe({
      next: (warehouses) => (this.warehouses = warehouses),
      error: () => this.notification.error('Nie udało się pobrać magazynów.'),
    });
  }

  get title(): string {
    if (this.data.mode === 'details') return 'Szczegóły lokalizacji';
    if (this.data.mode === 'delete') return 'Usuń lokalizację';
    return this.data.row ? 'Edytuj lokalizację' : 'Dodaj lokalizację';
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
