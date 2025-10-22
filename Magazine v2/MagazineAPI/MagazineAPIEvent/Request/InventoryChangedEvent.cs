using Mediator;

namespace MagazineAPIEvent.Request;

public sealed record InventoryChangedEvent(
    Guid ProductId, Guid LocationId, decimal Quantity, decimal ReservedQuantity, Guid AggregateId, decimal QuantityChange
    ): INotification;