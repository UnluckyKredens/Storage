export interface WarehouseDashboardAlert {
  type: string;
  warehouseId: string;
  warehouseName: string;
  productId: string;
  productName: string;
  availableQuantity: number;
  minimumQuantity: number;
  optimumQuantity: number | null;
  message: string;
}

export interface WarehouseDashboard {
  productCount: number;
  inventoryItemCount: number;
  totalQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  activeReservationCount: number;
  pendingShipmentCount: number;
  sentShipmentCount: number;
  inTransitShipmentCount: number;
  alerts: WarehouseDashboardAlert[];
}

export interface WarehouseFlowDay {
  date: string;
  inbound: number;
  outbound: number;
}

export interface WarehouseFlowProduct {
  productId: string;
  productName: string;
  inbound: number;
  outbound: number;
}

export interface WarehouseShipmentFlow {
  created: number;
  requested: number;
  approved: number;
  inTransit: number;
  received: number;
  cancelled: number;
  averageApprovalHours: number | null;
  averageDeliveryHours: number | null;
}

export interface WarehouseFlowStats {
  days: number;
  fromUtc: string;
  toUtc: string;
  inboundQuantity: number;
  outboundQuantity: number;
  netChange: number;
  movementCount: number;
  daily: WarehouseFlowDay[];
  topProducts: WarehouseFlowProduct[];
  shipments: WarehouseShipmentFlow;
}
