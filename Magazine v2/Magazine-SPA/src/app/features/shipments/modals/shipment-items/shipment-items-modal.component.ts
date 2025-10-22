import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../../shared/components/dialog-header/dialog-header.component';
import { Shipment } from '../../models/shipments.models';

interface ShipmentItemsModalData {
  shipment: Shipment;
}

@Component({
  selector: 'app-shipment-items-modal',
  imports: [MatButtonModule, MatDialogModule, DialogHeaderComponent],
  templateUrl: './shipment-items-modal.component.html',
  styleUrls: [
    '../../../../shared/styles/management-modal.scss',
    './shipment-items-modal.component.scss',
  ],
})
export class ShipmentItemsModalComponent {
  readonly data = inject<ShipmentItemsModalData>(MAT_DIALOG_DATA);
}
