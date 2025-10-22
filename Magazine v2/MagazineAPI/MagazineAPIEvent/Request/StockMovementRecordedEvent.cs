using MagazineAPIDomain.Enums;
using Mediator;

namespace MagazineAPIEvent.Request;

public sealed record StockMovementRecordedEvent(
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
    string? Notes) : INotification;
