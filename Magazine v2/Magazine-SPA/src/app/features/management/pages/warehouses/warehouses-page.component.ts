import { Component, inject } from '@angular/core';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { WarehouseModalComponent } from '../../modals/warehouse/warehouse-modal.component';
import { Warehouse } from '../../../../shared/models/master-data.models';
import { WarehouseService } from '../../services/warehouse.service';

@Component({
  selector: 'app-warehouses-page',
  imports: entityListPageImports,
  templateUrl: './warehouses-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class WarehousesPageComponent extends EntityListPage<Warehouse> {
  protected readonly service = inject(WarehouseService);
  protected readonly config = {
    managePermission: 'warehouses.manage',
    modal: WarehouseModalComponent,
    added: 'Magazyn dodany.',
    updated: 'Magazyn został zaktualizowany.',
    removed: 'Magazyn usunięty.',
    loadError: 'Nie udało się pobrać magazynów.',
  };
  readonly displayedColumns = ['name', 'address', 'description', 'actions'];
}
