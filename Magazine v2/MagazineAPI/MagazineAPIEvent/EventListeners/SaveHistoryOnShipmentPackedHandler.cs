using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnShipmentPackedHandler(
    IRepository<ShipmentHistory> shipmentHistoryRepository)
    : INotificationHandler<ShipmentPackedEvent>
{
    public async ValueTask Handle(
        ShipmentPackedEvent notification,
        CancellationToken cancellationToken)
    {
        await shipmentHistoryRepository.AddAsync(new ShipmentHistory
        {
            ShipmentId = notification.ShipmentId,
            EventType = "packed",
            SourceWarehouseId = notification.SourceWarehouseId,
            DestinationWarehouseId = notification.DestinationWarehouseId,
            UserId = notification.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Details = $"Spakowano wysyłkę {notification.Number}, pozycji: {notification.ItemCount}."
        }, cancellationToken);
    }
}
