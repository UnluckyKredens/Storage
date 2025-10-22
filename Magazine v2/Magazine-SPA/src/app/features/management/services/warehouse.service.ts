import { Injectable } from '@angular/core';
import { EntityApiService } from '../../../core/entity/entity-api.service';
import { Warehouse } from '../../../shared/models/master-data.models';

@Injectable({ providedIn: 'root' })
export class WarehouseService extends EntityApiService<
  Warehouse,
  { name: string; address: string; taxNumber: string; description: string; isHub?: boolean }
> {
  constructor() {
    super('Warehouses');
  }
}
