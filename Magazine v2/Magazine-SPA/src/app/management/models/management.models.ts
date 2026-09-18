export interface SelectOption {
  value: string;
  label: string;
}

export interface Product {
  productId: string;
  categoryId: string;
  unitOfMeasureId: string;
  name: string;
  sku: string;
  barcode: string | null;
  description: string | null;
  category: string;
  unitOfMeasure: string;
  purchasePrice: number;
  salePrice: number;
  minimumQuantity: number;
  optimumQuantity: number | null;
  isActive: boolean;
}

export interface Category {
  id: string;
  name: string;
  description: string | null;
}

export interface UnitOfMeasure {
  id: string;
  name: string;
  symbol: string;
}

export interface Warehouse {
  id: string;
  name: string;
  address: string | null;
  description: string | null;
}

export interface Location {
  id: string;
  warehouseId: string;
  warehouseName: string;
  locationCode: string;
  description: string | null;
}

export interface Contractor {
  id: string;
  name: string;
  taxNumber: string;
  type: number;
  email: string | null;
  phone: string | null;
  address: string | null;
}

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

export interface AuditLog {
  id: string;
  createdOnUtc: string;
  userId: string | null;
  userName: string | null;
  action: string;
  entityName: string;
  entityId: string | null;
  summary: string | null;
  beforeValuesJson: string | null;
  afterValuesJson: string | null;
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

export interface User {
  id: string;
  login: string;
  firstName: string;
  lastName: string;
  email: string;
  roleId: string;
  roleName: string;
  warehouseId: string | null;
  warehouseName: string | null;
}

export interface Role {
  id: string;
  name: string;
  hasAllPermissions: boolean;
  permissionCodes: string[];
}

export interface Permission {
  id: string;
  code: string;
  name: string;
  description: string | null;
}

export interface ProductPage {
  list: Product[];
  total: number;
}

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
