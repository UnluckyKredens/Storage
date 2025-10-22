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
