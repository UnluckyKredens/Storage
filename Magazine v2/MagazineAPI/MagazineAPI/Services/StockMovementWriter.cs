using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;

namespace MagazineAPI.Services;

public sealed class StockMovementWriter(
    IRepository<StockMovement> movementRepository,
    ICurrentUser currentUser) : IStockMovementWriter
{
    public async Task RecordAsync(
        StockMovementRecord record,
        CancellationToken cancellationToken = default)
    {
        var movement = new StockMovement
        {
            StockMovementId = Guid.NewGuid(),
            WarehouseId = record.WarehouseId,
            LocationId = record.LocationId,
            ProductId = record.ProductId,
            InventoryId = record.InventoryId,
            Type = record.Type,
            QuantityBefore = record.QuantityBefore,
            QuantityChange = record.QuantityChange,
            QuantityAfter = record.QuantityAfter,
            ReservedQuantityBefore = record.ReservedQuantityBefore,
            ReservedQuantityChange = record.ReservedQuantityChange,
            ReservedQuantityAfter = record.ReservedQuantityAfter,
            SourceType = record.SourceType,
            SourceId = record.SourceId,
            SourceNumber = record.SourceNumber,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = DateTime.UtcNow,
            Notes = record.Notes
        };

        await movementRepository.AddAsync(movement, cancellationToken);
    }
}
