namespace MagazineAPIDomain.Authorization;

public static class PermissionCodes
{
    // Uprawnienie systemowe: nie występuje w katalogu i jest dostępne wyłącznie
    // przez administracyjny bypass w PermissionAuthorizationHandler.
    public const string RolesManage = "roles.manage";

    public const string ProductsRead = "products.read";
    public const string ProductsManage = "products.manage";
    public const string InventoryRead = "inventory.read";
    public const string InventoryManage = "inventory.manage";
    public const string WarehousesRead = "warehouses.read";
    public const string WarehousesManage = "warehouses.manage";
    public const string ContractorsRead = "contractors.read";
    public const string ContractorsManage = "contractors.manage";
    public const string UsersRead = "users.read";
    public const string UsersManage = "users.manage";
    public const string DictionariesManage = "dictionaries.manage";
    public const string StockDocumentsManage = "stock-documents.manage";
    public const string StockDocumentsApprove = "stock-documents.approve";
    public const string StockDocumentsRead = "stock-documents.read";
    public const string StockDocumentsReceive = "stock-documents.receive";
    public const string StockShipmentsCreate = "stock-shipments.create";
}
