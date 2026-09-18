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
