import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../../shared/components/dialog-header/dialog-header.component';
import {
  CloseDialogActionsComponent,
  DeleteDialogBodyComponent,
  FormDialogActionsComponent,
} from '../../../../shared/components/dialog-parts/dialog-parts.component';
import { EntityDialog } from '../../../../core/entity/entity-dialog';
import { FormInputComponent } from '../../../../shared/components/form-input/form-input.component';
import { FormSelectComponent } from '../../../../shared/components/form-select/form-select.component';
import { FormTextareaComponent } from '../../../../shared/components/form-textarea/form-textarea.component';
import { Location, SelectOption } from '../../../../shared/models/master-data.models';
import { LocationService } from '../../services/location.service';

@Component({
  selector: 'app-location-modal',
  imports: [
    FormsModule,
    MatDialogModule,
    DialogHeaderComponent,
    CloseDialogActionsComponent,
    DeleteDialogBodyComponent,
    FormDialogActionsComponent,
    FormInputComponent,
    FormSelectComponent,
    FormTextareaComponent,
  ],
  templateUrl: './location-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class LocationModalComponent extends EntityDialog<Location> implements OnInit {
  private readonly service = inject(LocationService);

  form = {
    warehouseId: String(this.data.row?.warehouseId ?? ''),
    locationCode: String(this.data.row?.locationCode ?? ''),
    description: String(this.data.row?.description ?? ''),
  };
  warehouses: SelectOption[] = [];

  protected readonly titles = {
    details: 'Szczegóły lokalizacji',
    delete: 'Usuń lokalizację',
    edit: 'Edytuj lokalizację',
    add: 'Dodaj lokalizację',
  };

  protected request() {
    return this.service.save(this.data.row?.id ?? null, this.form);
  }

  ngOnInit(): void {
    if (this.data.mode !== 'form') return;
    this.service.getFormOptions().subscribe({
      next: (warehouses) => (this.warehouses = warehouses),
      error: () => this.notification.error('Nie udało się pobrać magazynów.'),
    });
  }
}
