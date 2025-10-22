import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { EntityApiService } from '../../../core/entity/entity-api.service';
import { Location, SelectOption, Warehouse } from '../../../shared/models/master-data.models';

@Injectable({ providedIn: 'root' })
export class LocationService extends EntityApiService<
  Location,
  { warehouseId: string; locationCode: string; description: string }
> {
  constructor() {
    super('Locations');
  }

  getFormOptions(): Observable<SelectOption[]> {
    return this.http
      .get<Warehouse[]>(`${this.apiUrl}/page-data`)
      .pipe(map((items) => items.map((item) => ({ value: item.id, label: item.name }))));
  }
}
