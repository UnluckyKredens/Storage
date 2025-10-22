using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.StockMovements;

// Runs synchronously inside the publisher's transaction, so the ledger entry commits or rolls back with the stock change.
public sealed class SaveStockMovementOnStockMovementRecordedHandler(
    IRepository<StockMovement> movementRepository,
    IWarehouseContext currentUser) : INotificationHandler<StockMovementRecordedEvent>
{
    public async ValueTask Handle(StockMovementRecordedEvent notification, CancellationToken cancellationToken)
    {
        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            WarehouseId = notification.WarehouseId,
            LocationId = notification.LocationId,
            ProductId = notification.ProductId,
            InventoryId = notification.InventoryId,
            Type = notification.Type,
            QuantityBefore = notification.QuantityBefore,
            QuantityChange = notification.QuantityChange,
            QuantityAfter = notification.QuantityAfter,
            ReservedQuantityBefore = notification.ReservedQuantityBefore,
            ReservedQuantityChange = notification.ReservedQuantityChange,
            ReservedQuantityAfter = notification.ReservedQuantityAfter,
            SourceType = notification.SourceType,
            SourceId = notification.SourceId,
            SourceNumber = notification.SourceNumber,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Notes = notification.Notes
        };

        await movementRepository.AddAsync(movement, cancellationToken);
    }
}
