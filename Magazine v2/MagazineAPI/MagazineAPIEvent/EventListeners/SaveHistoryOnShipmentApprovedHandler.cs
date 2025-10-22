using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnShipmentApprovedHandler(
    IRepository<ShipmentHistory> shipmentHistoryRepository)
    : INotificationHandler<ShipmentApprovedEvent>
{
    public async ValueTask Handle(
        ShipmentApprovedEvent notification,
        CancellationToken cancellationToken)
    {
        await shipmentHistoryRepository.AddAsync(new ShipmentHistory
        {
            ShipmentId = notification.ShipmentId,
            EventType = "approved",
            SourceWarehouseId = notification.SourceWarehouseId,
            DestinationWarehouseId = notification.DestinationWarehouseId,
            UserId = notification.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Details = $"Wysłano wysyłkę {notification.Number}, pozycji: {notification.ItemCount}."
        }, cancellationToken);
    }
}
