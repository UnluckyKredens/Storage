using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnShipmentReceivedHandler(
    IRepository<ShipmentHistory> shipmentHistoryRepository)
    : INotificationHandler<ShipmentReceivedEvent>
{
    public async ValueTask Handle(
        ShipmentReceivedEvent notification,
        CancellationToken cancellationToken)
    {
        var details = $"Odebrano wysyłkę {notification.Number}, pozycji: {notification.ItemCount}.";
        if (!string.IsNullOrWhiteSpace(notification.Notes))
            details = $"{details} Uwagi z odbioru: {notification.Notes.Trim()}";

        await shipmentHistoryRepository.AddAsync(new ShipmentHistory
        {
            ShipmentId = notification.ShipmentId,
            EventType = "received",
            SourceWarehouseId = notification.SourceWarehouseId,
            DestinationWarehouseId = notification.DestinationWarehouseId,
            UserId = notification.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Details = details
        }, cancellationToken);
    }
}
