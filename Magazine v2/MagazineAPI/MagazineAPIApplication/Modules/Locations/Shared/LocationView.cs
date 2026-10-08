namespace MagazineAPIApplication.Modules.Locations;

public sealed record LocationView(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    string LocationCode,
    string? Description);
