export enum StockDocumentType {
  Receipt = 1,
  Shipment = 2,
}

export enum StockDocumentStatus {
  Draft = 1,
  Completed = 2,
  Received = 3,
}

export interface StockDocumentListItem {
  id: string;
  number: string;
  type: StockDocumentType;
  typeName: string;
  status: StockDocumentStatus;
  statusName: string;
  warehouseId: string;
  warehouseName: string;
  contractorId: string | null;
  contractorName: string | null;
  destinationWarehouseId: string | null;
  destinationWarehouseName: string | null;
  createdAtUtc: string;
  completedAtUtc: string | null;
  receivedAtUtc: string | null;
  receivedBy: string | null;
  approvedBy: string | null;
  itemsCount: number;
}

export interface StockDocumentItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  locationId: string;
  locationCode: string;
  quantity: number;
}

export interface StockDocumentDetails extends StockDocumentListItem {
  createdByUserId: string;
  createdBy: string;
  approvedByUserId: string | null;
  receivedByUserId: string | null;
  notes: string | null;
  items: StockDocumentItem[];
}

export interface StockDocumentPage {
  list: StockDocumentListItem[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ProductOption {
  id: string;
  name: string;
  sku: string;
}

export interface LocationOption {
  id: string;
  code: string;
  warehouseId: string;
  warehouseName: string;
}

export interface WarehouseOption {
  id: string;
  name: string;
}

export interface ContractorOption {
  id: string;
  name: string;
  type: number;
}

export interface StockDocumentPageData {
  products: ProductOption[];
  locations: LocationOption[];
  warehouses: WarehouseOption[];
  destinationWarehouses: WarehouseOption[];
  contractors: ContractorOption[];
}

export interface StockDocumentItemForm {
  productId: string;
  locationId: string;
  quantity: number;
}

export interface SaveStockDocument {
  id: string | null;
  type: StockDocumentType;
  warehouseId: string;
  contractorId: string | null;
  destinationWarehouseId: string | null;
  notes: string | null;
  items: StockDocumentItemForm[];
}
