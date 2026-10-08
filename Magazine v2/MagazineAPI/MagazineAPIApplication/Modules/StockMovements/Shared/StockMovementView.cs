using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.StockMovements;

public sealed record StockMovementView(
    Guid Id,
    DateTime CreatedOnUtc,
    StockMovementType Type,
    Guid WarehouseId,
    string WarehouseName,
    Guid LocationId,
    string LocationCode,
    Guid ProductId,
    string ProductName,
    Guid? InventoryId,
    decimal QuantityBefore,
    decimal QuantityChange,
    decimal QuantityAfter,
    decimal ReservedQuantityBefore,
    decimal ReservedQuantityChange,
    decimal ReservedQuantityAfter,
    string? SourceType,
    Guid? SourceId,
    string? SourceNumber,
    Guid CreatedByUserId,
    string CreatedBy,
    string? Notes);
