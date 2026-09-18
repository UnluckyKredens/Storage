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
