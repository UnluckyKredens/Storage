import { Warehouse } from '../../management/models/management.models';

export interface ShipmentItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  barcode: string;
  quantity: number;
}

export interface Shipment {
  id: string;
  number: string;
  sourceWarehouseId: string;
  sourceWarehouseName: string;
  destinationWarehouseId: string;
  destinationWarehouseName: string;
  status: string;
  createdOnUtc: string;
  approvedOnUtc: string | null;
  receivedOnUtc: string | null;
  items: ShipmentItem[];
  sourceWarehouseAddress: string | null;
  sourceWarehouseTaxNumber: string | null;
  destinationWarehouseAddress: string | null;
  destinationWarehouseTaxNumber: string | null;
  createdBy: string | null;
  approvedBy: string | null;
  packedBy: string | null;
  packedOnUtc: string | null;
  sentBy: string | null;
  sentOnUtc: string | null;
  receivedBy: string | null;
}

export interface ShipmentHistory {
  id: string;
  eventType: string;
  eventName: string;
  sourceWarehouseId: string;
  sourceWarehouseName: string;
  destinationWarehouseId: string;
  destinationWarehouseName: string;
  userId: string | null;
  userName: string | null;
  createdOnUtc: string;
  details: string;
}

export interface ShipmentPageData {
  sourceWarehouse: Warehouse;
  destinationWarehouses: Warehouse[];
  availableProducts: ShipmentProduct[];
}

export interface ShipmentProduct {
  productId: string;
  name: string;
  sku: string;
  barcode: string;
  availableQuantity: number;
}
