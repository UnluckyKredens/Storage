namespace MagazineAPIApplication.Modules.Inventories;

public sealed record InventoryView(
    Guid Id, Guid ProductId, string ProductName, Guid LocationId,
    string LocationCode, Guid WarehouseId, string WarehouseName,
    decimal Quantity, decimal ReservedQuantity, decimal AvailableQuantity);
