namespace MagazineAPIApplication.Modules.Warehouses;

public sealed record WarehouseView(
    Guid Id,
    string Name,
    string? Address,
    string? Description,
    bool IsHub,
    string? TaxNumber = null);
