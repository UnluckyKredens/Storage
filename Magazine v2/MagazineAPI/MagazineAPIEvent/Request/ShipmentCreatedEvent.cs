using Mediator;

namespace MagazineAPIEvent.Request;

public sealed record ShipmentCreatedEvent(
    Guid ShipmentId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid UserId,
    string Number,
    int ItemCount) : INotification;
