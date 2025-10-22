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
import {
  FormSelectComponent,
  FormSelectOption,
} from '../../../../shared/components/form-select/form-select.component';
import { FormTextareaComponent } from '../../../../shared/components/form-textarea/form-textarea.component';
import { Contractor } from '../../../../shared/models/master-data.models';
import { ContractorService } from '../../services/contractor.service';

@Component({
  selector: 'app-contractor-modal',
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
  templateUrl: './contractor-modal.component.html',
  styleUrl: '../../../../shared/styles/management-modal.scss',
})
export class ContractorModalComponent extends EntityDialog<Contractor> {
  private readonly service = inject(ContractorService);

  form = {
    name: String(this.data.row?.name ?? ''),
    taxNumber: String(this.data.row?.taxNumber ?? ''),
    type: Number(this.data.row?.type ?? 1),
    email: String(this.data.row?.email ?? ''),
    phone: String(this.data.row?.phone ?? ''),
    address: String(this.data.row?.address ?? ''),
  };
  contractorTypes: FormSelectOption[] = [
    { value: 1, label: 'Dostawca' },
    { value: 2, label: 'Odbiorca' },
    { value: 3, label: 'Oba' },
  ];

  contractorType(): string {
    const type = Number(this.data.row?.type);
    if (type === 1) return 'Dostawca';
    if (type === 2) return 'Odbiorca';
    if (type === 3) return 'Oba';
    return '—';
  }

  protected readonly titles = {
    details: 'Szczegóły kontrahenta',
    delete: 'Usuń kontrahenta',
    edit: 'Edytuj kontrahenta',
    add: 'Dodaj kontrahenta',
  };

  protected request() {
    return this.service.save(this.data.row?.id ?? null, this.form);
  }
}
