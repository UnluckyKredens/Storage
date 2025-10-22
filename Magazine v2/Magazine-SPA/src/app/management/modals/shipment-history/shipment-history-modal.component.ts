import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { DialogHeaderComponent } from '../../../shared/components/dialog-header/dialog-header.component';
import { Shipment, ShipmentHistory } from '../../models/management.models';

interface ShipmentHistoryModalData {
  shipment: Shipment;
  history: ShipmentHistory[];
}

@Component({
  selector: 'app-shipment-history-modal',
  imports: [DatePipe, MatButtonModule, MatDialogModule, DialogHeaderComponent],
  templateUrl: './shipment-history-modal.component.html',
  styleUrls: ['../management-modal.scss', './shipment-history-modal.component.scss'],
})
export class ShipmentHistoryModalComponent {
  readonly data = inject<ShipmentHistoryModalData>(MAT_DIALOG_DATA);
}
