namespace MagazineAPIApplication.Modules.WarehouseOperations;

public sealed record WarehouseOperationView(
    Guid Id,
    string Number,
    string Type,
    string Status,
    Guid WarehouseId,
    string WarehouseName,
    DateTime CompletedOnUtc,
    string? Notes,
    IReadOnlyList<WarehouseOperationItemView> Items);

public sealed record WarehouseOperationItemView(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? SourceLocationCode,
    string? DestinationLocationCode,
    decimal Quantity,
    decimal? TargetQuantity);
