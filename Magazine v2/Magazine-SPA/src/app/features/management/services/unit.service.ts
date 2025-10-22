import { Injectable } from '@angular/core';
import { EntityApiService } from '../../../core/entity/entity-api.service';
import { UnitOfMeasure } from '../../../shared/models/master-data.models';

@Injectable({ providedIn: 'root' })
export class UnitService extends EntityApiService<UnitOfMeasure, { name: string; symbol: string }> {
  constructor() {
    super('UnitsOfMeasure');
  }
}
