using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnShipmentInTransitHandler(
    IRepository<ShipmentHistory> shipmentHistoryRepository)
    : INotificationHandler<ShipmentInTransitEvent>
{
    public async ValueTask Handle(
        ShipmentInTransitEvent notification,
        CancellationToken cancellationToken)
    {
        await shipmentHistoryRepository.AddAsync(new ShipmentHistory
        {
            ShipmentId = notification.ShipmentId,
            EventType = "in_transit",
            SourceWarehouseId = notification.SourceWarehouseId,
            DestinationWarehouseId = notification.DestinationWarehouseId,
            UserId = notification.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Details = $"Wysyłka {notification.Number} jest w drodze."
        }, cancellationToken);
    }
}
