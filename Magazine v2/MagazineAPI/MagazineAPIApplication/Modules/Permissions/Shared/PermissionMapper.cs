using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Modules.Permissions;

internal static class PermissionMapper
{
    public static PermissionView ToView(Permission item) => new(item.Id, item.Code, item.Name, item.Description);
    public static bool IsSystemPermission(Guid id) => new[]
    {
        PermissionIds.ProductsRead, PermissionIds.ProductsManage,
        PermissionIds.InventoryRead, PermissionIds.InventoryManage,
        PermissionIds.WarehousesRead, PermissionIds.WarehousesManage,
        PermissionIds.ContractorsRead, PermissionIds.ContractorsManage,
        PermissionIds.UsersRead, PermissionIds.UsersManage, PermissionIds.DictionariesManage,
        PermissionIds.StockDocumentsManage, PermissionIds.StockDocumentsApprove,
        PermissionIds.StockDocumentsRead, PermissionIds.StockDocumentsReceive,
        PermissionIds.StockShipmentsCreate
    }.Contains(id);
}
