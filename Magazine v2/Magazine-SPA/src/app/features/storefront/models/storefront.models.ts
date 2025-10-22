export interface StorefrontPackingAllocation {
  locationCode: string;
  quantity: number;
  consumedQuantity: number;
}

export interface StorefrontPackingOrderItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  stockQuantity: number;
  allocations: StorefrontPackingAllocation[];
}

export interface StorefrontPackingOrder {
  id: string;
  number: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string | null;
  deliveryAddress: string | null;
  assignedWarehouseId: string | null;
  assignedWarehouseName: string;
  status: string;
  currentLocation: string;
  createdOnUtc: string;
  acceptedOnUtc: string | null;
  completedOnUtc: string | null;
  totalValue: number;
  items: StorefrontPackingOrderItem[];
}

export interface StorefrontOrderHistoryItem {
  productName: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface StorefrontOrderHistory {
  id: string;
  number: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string | null;
  deliveryAddress: string | null;
  notes: string | null;
  assignedWarehouseName: string;
  assignedWarehouseAddress: string | null;
  assignedWarehouseTaxNumber: string | null;
  status: string;
  createdOnUtc: string;
  acceptedOnUtc: string | null;
  acceptedBy: string | null;
  packedOnUtc: string | null;
  packedBy: string | null;
  shippedOnUtc: string | null;
  shippedBy: string | null;
  carrierName: string | null;
  carrierTrackingNumber: string | null;
  totalValue: number;
  items: StorefrontOrderHistoryItem[];
}

export interface StorefrontPoolItem {
  productName: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  availableHere: number;
}

export interface StorefrontPoolOrder {
  id: string;
  number: string;
  customerName: string;
  deliveryAddress: string | null;
  notes: string | null;
  createdOnUtc: string;
  expiresOnUtc: string;
  totalValue: number;
  canFulfil: boolean;
  items: StorefrontPoolItem[];
}
