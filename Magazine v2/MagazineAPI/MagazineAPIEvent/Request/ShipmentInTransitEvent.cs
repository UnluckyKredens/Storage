using Mediator;

namespace MagazineAPIEvent.Request;

public sealed record ShipmentInTransitEvent(
    Guid ShipmentId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid? UserId,
    string Number) : INotification;
