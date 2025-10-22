import { Component, inject } from '@angular/core';
import { EntityListPage, entityListPageImports } from '../../../../core/entity/entity-list-page';
import { LocationModalComponent } from '../../modals/location/location-modal.component';
import { Location } from '../../../../shared/models/master-data.models';
import { LocationService } from '../../services/location.service';

@Component({
  selector: 'app-locations-page',
  imports: entityListPageImports,
  templateUrl: './locations-page.component.html',
  styleUrl: '../../../../shared/styles/management-page.scss',
})
export class LocationsPageComponent extends EntityListPage<Location> {
  protected readonly service = inject(LocationService);
  protected readonly config = {
    managePermission: 'warehouses.manage',
    modal: LocationModalComponent,
    added: 'Lokalizacja dodana.',
    updated: 'Lokalizacja została zaktualizowana.',
    removed: 'Lokalizacja usunięta.',
    loadError: 'Nie udało się pobrać lokalizacji.',
  };
  protected override readonly reloadOnWarehouseChange = true;
  readonly displayedColumns = ['locationCode', 'warehouseName', 'description', 'actions'];
}
