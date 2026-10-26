export interface InventoryItem {
  id: string;
  productId: string;
  productName: string;
  locationId: string;
  locationCode: string;
  warehouseId: string;
  warehouseName: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
}

export interface StockMovement {
  id: string;
  createdOnUtc: string;
  type: number;
  warehouseId: string;
  warehouseName: string;
  locationId: string;
  locationCode: string;
  productId: string;
  productName: string;
  inventoryId: string | null;
  quantityBefore: number;
  quantityChange: number;
  quantityAfter: number;
  reservedQuantityBefore: number;
  reservedQuantityChange: number;
  reservedQuantityAfter: number;
  sourceType: string | null;
  sourceId: string | null;
  sourceNumber: string | null;
  createdByUserId: string;
  createdBy: string;
  notes: string | null;
}

export interface WarehouseOperationItem {
  id: string;
  productId: string;
  productName: string;
  sourceLocationCode: string | null;
  destinationLocationCode: string | null;
  quantity: number;
  targetQuantity: number | null;
}

export interface WarehouseOperation {
  id: string;
  number: string;
  type: string;
  status: string;
  warehouseId: string;
  warehouseName: string;
  completedOnUtc: string;
  notes: string | null;
  items: WarehouseOperationItem[];
}
