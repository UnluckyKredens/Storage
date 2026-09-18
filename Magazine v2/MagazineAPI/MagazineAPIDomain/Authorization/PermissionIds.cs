namespace MagazineAPIDomain.Authorization;

public static class PermissionIds
{
    public static readonly Guid ProductsRead = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid ProductsManage = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid InventoryRead = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid InventoryManage = Guid.Parse("20000000-0000-0000-0000-000000000004");
    public static readonly Guid WarehousesRead = Guid.Parse("20000000-0000-0000-0000-000000000005");
    public static readonly Guid WarehousesManage = Guid.Parse("20000000-0000-0000-0000-000000000006");
    public static readonly Guid ContractorsRead = Guid.Parse("20000000-0000-0000-0000-000000000007");
    public static readonly Guid ContractorsManage = Guid.Parse("20000000-0000-0000-0000-000000000008");
    public static readonly Guid UsersRead = Guid.Parse("20000000-0000-0000-0000-000000000009");
    public static readonly Guid UsersManage = Guid.Parse("20000000-0000-0000-0000-000000000010");
    public static readonly Guid DictionariesManage = Guid.Parse("20000000-0000-0000-0000-000000000011");
    public static readonly Guid ShipmentsRead = Guid.Parse("20000000-0000-0000-0000-000000000012");
    public static readonly Guid ShipmentsCreate = Guid.Parse("20000000-0000-0000-0000-000000000013");
    public static readonly Guid ShipmentsApprove = Guid.Parse("20000000-0000-0000-0000-000000000014");
}
