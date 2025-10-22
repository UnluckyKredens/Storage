using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnShipmentCreatedHandler(
    IRepository<ShipmentHistory> shipmentHistoryRepository)
    : INotificationHandler<ShipmentCreatedEvent>
{
    public async ValueTask Handle(
        ShipmentCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        await shipmentHistoryRepository.AddAsync(new ShipmentHistory
        {
            ShipmentId = notification.ShipmentId,
            EventType = "created",
            SourceWarehouseId = notification.SourceWarehouseId,
            DestinationWarehouseId = notification.DestinationWarehouseId,
            UserId = notification.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Details = $"Utworzono wysyłkę {notification.Number}, pozycji: {notification.ItemCount}."
        }, cancellationToken);
    }
}
