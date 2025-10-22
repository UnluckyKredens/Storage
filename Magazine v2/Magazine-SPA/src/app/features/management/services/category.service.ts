import { Injectable } from '@angular/core';
import { EntityApiService } from '../../../core/entity/entity-api.service';
import { Category } from '../../../shared/models/master-data.models';

@Injectable({ providedIn: 'root' })
export class CategoryService extends EntityApiService<
  Category,
  { name: string; description: string }
> {
  constructor() {
    super('Category');
  }
}
