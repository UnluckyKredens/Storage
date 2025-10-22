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
import { FormTextareaComponent } from '../../../../shared/components/form-textarea/form-textarea.component';
import { Permission } from '../../models/management.models';
import { PermissionService } from '../../services/permission.service';

@Component({
  selector: 'app-permission-modal',
  imports: [
    FormsModule,
    MatDialogModule,
    DialogHeaderComponent,
    CloseDialogActionsComponent,
    DeleteDialogBodyComponent,
    FormDialogActionsComponent,
    FormInputComponent,
    FormTextareaComponent,
  ],
  templateUrl: './permission-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class PermissionModalComponent extends EntityDialog<Permission> {
  private readonly service = inject(PermissionService);

  form = {
    code: String(this.data.row?.code ?? ''),
    name: String(this.data.row?.name ?? ''),
    description: String(this.data.row?.description ?? ''),
  };

  protected readonly titles = {
    details: 'Szczegóły uprawnienia',
    delete: 'Usuń uprawnienie',
    edit: 'Edytuj uprawnienie',
    add: 'Dodaj uprawnienie',
  };

  protected request() {
    return this.service.save(this.data.row?.id ?? null, this.form);
  }
}
