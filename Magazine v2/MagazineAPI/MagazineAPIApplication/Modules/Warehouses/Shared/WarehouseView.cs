namespace MagazineAPIApplication.Modules.Warehouses;

public sealed record WarehouseView(
    Guid Id,
    string Name,
    string? Address,
    string? Description);
