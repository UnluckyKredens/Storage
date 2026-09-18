using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Common.Interfaces;

public sealed record StockMovementRecord(
    Guid WarehouseId,
    Guid LocationId,
    Guid ProductId,
    Guid? InventoryId,
    StockMovementType Type,
    decimal QuantityBefore,
    decimal QuantityChange,
    decimal QuantityAfter,
    decimal ReservedQuantityBefore,
    decimal ReservedQuantityChange,
    decimal ReservedQuantityAfter,
    string? SourceType,
    Guid? SourceId,
    string? SourceNumber,
    string? Notes);

public interface IStockMovementWriter
{
    Task RecordAsync(StockMovementRecord record, CancellationToken cancellationToken = default);
}
