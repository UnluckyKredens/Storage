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
import { administratorRoleId } from '../../../../core/auth/roles';
import { AccountService } from '../../../../core/auth/services/account.service';
import { FormCheckboxComponent } from '../../../../shared/components/form-checkbox/form-checkbox.component';
import { FormInputComponent } from '../../../../shared/components/form-input/form-input.component';
import { FormTextareaComponent } from '../../../../shared/components/form-textarea/form-textarea.component';
import { Warehouse } from '../../../../shared/models/master-data.models';
import { WarehouseService } from '../../services/warehouse.service';

@Component({
  selector: 'app-warehouse-modal',
  imports: [
    FormsModule,
    MatDialogModule,
    DialogHeaderComponent,
    CloseDialogActionsComponent,
    DeleteDialogBodyComponent,
    FormDialogActionsComponent,
    FormInputComponent,
    FormTextareaComponent,
    FormCheckboxComponent,
  ],
  templateUrl: './warehouse-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class WarehouseModalComponent extends EntityDialog<Warehouse> {
  private readonly service = inject(WarehouseService);

  form = {
    name: String(this.data.row?.name ?? ''),
    address: String(this.data.row?.address ?? ''),
    taxNumber: String(this.data.row?.taxNumber ?? ''),
    description: String(this.data.row?.description ?? ''),
    isHub: Boolean(this.data.row?.isHub),
  };
  readonly isAdministrator = inject(AccountService).user()?.roleId === administratorRoleId;

  protected readonly titles = {
    details: 'Szczegóły magazynu',
    delete: 'Usuń magazyn',
    edit: 'Edytuj magazyn',
    add: 'Dodaj magazyn',
  };

  protected request() {
    return this.service.save(this.data.row?.id ?? null, this.form);
  }
}
