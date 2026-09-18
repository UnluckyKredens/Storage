using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;

namespace MagazineAPI.Services;

public sealed class ShipmentReservationService(
    IRepository<Location> locationRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<StockReservation> reservationRepository,
    ICurrentUser currentUser,
    IStockMovementWriter stockMovementWriter) : IShipmentReservationService
{
    public async Task ReserveAsync(
        Shipment shipment,
        IReadOnlyCollection<ShipmentItem> items,
        string note,
        CancellationToken cancellationToken = default)
    {
        var sourceLocations = await locationRepository.FilterByAsync(
            location => location.WarehouseId == shipment.SourceWarehouseId,
            cancellationToken);
        var sourceLocationIds = sourceLocations
            .Select(location => location.LocationId)
            .ToHashSet();
        var locationById = sourceLocations.ToDictionary(location => location.LocationId);

        foreach (var item in items)
        {
            await ReserveItemAsync(shipment, item, sourceLocationIds, locationById, note, cancellationToken);
        }
    }

    private async Task ReserveItemAsync(
        Shipment shipment,
        ShipmentItem item,
        IReadOnlySet<Guid> sourceLocationIds,
        IReadOnlyDictionary<Guid, Location> locationById,
        string note,
        CancellationToken cancellationToken)
    {
        var inventories = (await inventoryRepository.FilterByAsync(
                inventory => inventory.ProductId == item.ProductId,
                cancellationToken))
            .Where(inventory => sourceLocationIds.Contains(inventory.LocationId))
            .OrderBy(inventory => locationById[inventory.LocationId].LocationCode)
            .ToList();

        var available = inventories.Sum(inventory => inventory.AvailableQuantity);
        if (available < item.Quantity)
            throw new CommandValidationException($"Brak wystarczającej ilości dla kodu {item.Barcode}.");

        var remaining = item.Quantity;
        foreach (var inventory in inventories)
        {
            if (remaining <= 0) break;
            var take = Math.Min(inventory.AvailableQuantity, remaining);
            if (take <= 0) continue;

            await ReserveInventorySliceAsync(shipment, item, inventory, take, note, cancellationToken);
            remaining -= take;
        }
    }

    private async Task ReserveInventorySliceAsync(
        Shipment shipment,
        ShipmentItem item,
        Inventory inventory,
        decimal quantity,
        string note,
        CancellationToken cancellationToken)
    {
        var quantityBefore = inventory.Quantity;
        var reservedQuantityBefore = inventory.ReservedQuantity;
        inventory.ReservedQuantity += quantity;
        await inventoryRepository.UpdateAsync(inventory, cancellationToken);

        await reservationRepository.AddAsync(new StockReservation
        {
            StockReservationId = Guid.NewGuid(),
            WarehouseId = shipment.SourceWarehouseId,
            LocationId = inventory.LocationId,
            ProductId = inventory.ProductId,
            InventoryId = inventory.InventoryId,
            Quantity = quantity,
            ReleasedQuantity = 0,
            Status = StockReservationStatus.Active,
            SourceType = "Shipment",
            SourceId = shipment.ShipmentId,
            SourceItemId = item.ShipmentItemId,
            SourceNumber = shipment.Number,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = DateTime.UtcNow
        }, cancellationToken);

        await stockMovementWriter.RecordAsync(
            new StockMovementRecord(
                shipment.SourceWarehouseId,
                inventory.LocationId,
                inventory.ProductId,
                inventory.InventoryId,
                StockMovementType.Reservation,
                quantityBefore,
                0,
                inventory.Quantity,
                reservedQuantityBefore,
                quantity,
                inventory.ReservedQuantity,
                "Shipment",
                shipment.ShipmentId,
                shipment.Number,
                note),
            cancellationToken);
    }
}
