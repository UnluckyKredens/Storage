using Mediator;

namespace MagazineAPIEvent.Request;

public sealed record ShipmentReceivedEvent(
    Guid ShipmentId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid UserId,
    string Number,
    int ItemCount,
    string? Notes) : INotification;
