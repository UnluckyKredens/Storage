import { Injectable } from '@angular/core';
import { EntityApiService } from '../../../core/entity/entity-api.service';
import { Contractor } from '../../../shared/models/master-data.models';

export interface ContractorForm {
  name: string;
  taxNumber: string;
  type: number;
  email: string;
  phone: string;
  address: string;
}

@Injectable({ providedIn: 'root' })
export class ContractorService extends EntityApiService<Contractor, ContractorForm> {
  constructor() {
    super('Contractors');
  }
}
