using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnShipmentAcceptedHandler(
    IRepository<ShipmentHistory> shipmentHistoryRepository)
    : INotificationHandler<ShipmentAcceptedEvent>
{
    public async ValueTask Handle(
        ShipmentAcceptedEvent notification,
        CancellationToken cancellationToken)
    {
        await shipmentHistoryRepository.AddAsync(new ShipmentHistory
        {
            ShipmentId = notification.ShipmentId,
            EventType = "accepted",
            SourceWarehouseId = notification.SourceWarehouseId,
            DestinationWarehouseId = notification.DestinationWarehouseId,
            UserId = notification.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Details = $"Zaakceptowano wysyłkę {notification.Number}, pozycji: {notification.ItemCount}."
        }, cancellationToken);
    }
}
