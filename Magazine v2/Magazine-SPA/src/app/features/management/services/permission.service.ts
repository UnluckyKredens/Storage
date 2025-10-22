import { Injectable } from '@angular/core';
import { EntityApiService } from '../../../core/entity/entity-api.service';
import { Permission } from '../models/management.models';

@Injectable({ providedIn: 'root' })
export class PermissionService extends EntityApiService<
  Permission,
  { code: string; name: string; description: string }
> {
  constructor() {
    super('Permissions');
  }
}
