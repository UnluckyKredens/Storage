export const administratorRoleId = '10000000-0000-0000-0000-000000000001';
export const managerRoleId = '10000000-0000-0000-0000-000000000002';
export const workerRoleId = '10000000-0000-0000-0000-000000000003';
export const isSystemRole = (id: string): boolean => [administratorRoleId, managerRoleId, workerRoleId].includes(id);
