import { Component, inject } from '@angular/core';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { ContractorModalComponent } from '../../modals/contractor/contractor-modal.component';
import { Contractor } from '../../../../shared/models/master-data.models';
import { ContractorService } from '../../services/contractor.service';

@Component({
  selector: 'app-contractors-page',
  imports: entityListPageImports,
  templateUrl: './contractors-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class ContractorsPageComponent extends EntityListPage<Contractor> {
  protected readonly service = inject(ContractorService);
  protected readonly config = {
    managePermission: 'contractors.manage',
    modal: ContractorModalComponent,
    added: 'Kontrahent dodany.',
    updated: 'Kontrahent został zaktualizowany.',
    removed: 'Kontrahent usunięty.',
    loadError: 'Nie udało się pobrać kontrahentów.',
  };
  readonly displayedColumns = ['name', 'taxNumber', 'type', 'email', 'phone', 'actions'];

  typeLabel(type: number): string {
    if (type === 1) return 'Dostawca';
    if (type === 2) return 'Odbiorca';
    return 'Oba';
  }
}
