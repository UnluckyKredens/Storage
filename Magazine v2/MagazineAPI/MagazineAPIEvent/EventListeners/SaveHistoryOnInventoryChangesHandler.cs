using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIEvent.EventListeners;

public sealed class SaveHistoryOnInventoryChangesHandler(
    IRepository<InventoryHistory> inventoryHistoryRepository
    )
    : INotificationHandler<InventoryChangedEvent>
{
    public async ValueTask Handle(
        InventoryChangedEvent notification,
        CancellationToken cancellationToken)
    {
        var record = new InventoryHistory()
        {
            AggregateId = notification.AggregateId,
            ItemId = notification.AggregateId,
            ProductId = notification.ProductId,
            LocationId = notification.LocationId,
            Quantity = notification.Quantity,
            QuantityChange = notification.QuantityChange,
            ReservedQuantity = notification.ReservedQuantity,
            CreatedOn = DateTime.UtcNow
        };

        await inventoryHistoryRepository.AddAsync(record, cancellationToken);
    }
}
