import { Warehouse } from '../../management/models/management.models';

export interface PurchaseOrderItem {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  barcode: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface PurchaseOrder {
  id: string;
  number: string;
  warehouseId: string;
  warehouseName: string;
  contractorId: string;
  contractorName: string;
  status: string;
  createdOnUtc: string;
  approvedOnUtc: string | null;
  receivedOnUtc: string | null;
  invoiceNumber: string | null;
  paperDocumentNumber: string | null;
  notes: string | null;
  totalValue: number;
  items: PurchaseOrderItem[];
  warehouseAddress: string | null;
  warehouseTaxNumber: string | null;
  contractorTaxNumber: string | null;
  contractorAddress: string | null;
  contractorEmail: string | null;
  contractorPhone: string | null;
  createdBy: string | null;
  approvedBy: string | null;
  receivedBy: string | null;
}

export interface PurchaseOrderContractor {
  id: string;
  name: string;
  taxNumber: string;
}

export interface PurchaseOrderProduct {
  productId: string;
  name: string;
  sku: string;
  barcode: string;
  purchasePrice: number;
}

export interface PurchaseOrderPageData {
  warehouse: Warehouse;
  suppliers: PurchaseOrderContractor[];
  products: PurchaseOrderProduct[];
  hubWarehouseName: string | null;
}
