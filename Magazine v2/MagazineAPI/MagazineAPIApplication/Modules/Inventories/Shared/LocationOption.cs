namespace MagazineAPIApplication.Modules.Inventories;

public sealed record LocationOption(Guid Id, string Code, Guid WarehouseId, string WarehouseName);
