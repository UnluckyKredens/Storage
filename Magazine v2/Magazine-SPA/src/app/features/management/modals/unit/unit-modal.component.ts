import { Component, inject } from '@angular/core';
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
import { UnitOfMeasure } from '../../../../shared/models/master-data.models';
import { UnitService } from '../../services/unit.service';

@Component({
  selector: 'app-unit-modal',
  imports: [
    FormsModule,
    MatDialogModule,
    DialogHeaderComponent,
    CloseDialogActionsComponent,
    DeleteDialogBodyComponent,
    FormDialogActionsComponent,
    FormInputComponent,
  ],
  templateUrl: './unit-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class UnitModalComponent extends EntityDialog<UnitOfMeasure> {
  private readonly service = inject(UnitService);

  form = {
    name: String(this.data.row?.name ?? ''),
    symbol: String(this.data.row?.symbol ?? ''),
  };

  protected readonly titles = {
    details: 'Szczegóły jednostki miary',
    delete: 'Usuń jednostkę miary',
    edit: 'Edytuj jednostkę miary',
    add: 'Dodaj jednostkę miary',
  };

  protected request() {
    return this.service.save(this.data.row?.id ?? null, this.form);
  }
}
