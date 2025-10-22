using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnShipmentCancelledHandler(IRepository<ShipmentHistory> histories)
    : INotificationHandler<ShipmentCancelledEvent>
{
    public async ValueTask Handle(ShipmentCancelledEvent notification, CancellationToken ct) =>
        _ = await histories.AddAsync(new ShipmentHistory {
            ShipmentId = notification.ShipmentId, SourceWarehouseId = notification.SourceWarehouseId,
            DestinationWarehouseId = notification.DestinationWarehouseId, UserId = notification.UserId,
            EventType = "cancelled", CreatedOnUtc = DateTime.UtcNow, Details = $"Anulowano wysyłkę {notification.Number}."
        }, ct);
}
