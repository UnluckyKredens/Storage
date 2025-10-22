using Mediator;

namespace MagazineAPIEvent.Request;

public sealed record ShipmentRequestedEvent(
    Guid ShipmentId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid UserId,
    string Number,
    int ItemCount) : INotification;
