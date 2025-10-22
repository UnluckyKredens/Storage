import { Component, inject } from '@angular/core';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { UnitModalComponent } from '../../modals/unit/unit-modal.component';
import { UnitOfMeasure } from '../../../../shared/models/master-data.models';
import { UnitService } from '../../services/unit.service';

@Component({
  selector: 'app-units-page',
  imports: entityListPageImports,
  templateUrl: './units-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class UnitsPageComponent extends EntityListPage<UnitOfMeasure> {
  protected readonly service = inject(UnitService);
  protected readonly config = {
    managePermission: 'dictionaries.manage',
    modal: UnitModalComponent,
    added: 'Jednostka miary dodana.',
    updated: 'Jednostka miary została zaktualizowana.',
    removed: 'Jednostka miary usunięta.',
    loadError: 'Nie udało się pobrać jednostek miary.',
  };
  readonly displayedColumns = ['name', 'symbol', 'actions'];
}
