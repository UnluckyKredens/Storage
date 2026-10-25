using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;
using MagazineAPIEvent.Request;

namespace MagazineAPI.Services;

public sealed class ShipmentReservationService(
    IRepository<Location> locationRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<StockReservation> reservationRepository,
    ICurrentUser currentUser, IPublisher publisher) : IShipmentReservationService
{
    public async Task ReserveAsync(
        Shipment shipment,
        IReadOnlyCollection<ShipmentItem> items,
        string note,
        CancellationToken cancellationToken = default,
        bool allowPartial = false)
    {
        var sourceLocations = await locationRepository.FilterByAsync(
            location => location.WarehouseId == shipment.SourceWarehouseId,
            cancellationToken);
        var sourceLocationIds = sourceLocations
            .Select(location => location.Id)
            .ToHashSet();
        var locationById = sourceLocations.ToDictionary(location => location.Id);

        foreach (var item in items)
        {
            await ReserveItemAsync(shipment, item, item.Quantity, allowPartial, sourceLocationIds, locationById, note, cancellationToken);
        }
    }

    public async Task ReserveMissingAsync(
        Shipment shipment,
        IReadOnlyCollection<ShipmentItem> items,
        string note,
        CancellationToken cancellationToken = default)
    {
        var sourceLocations = await locationRepository.FilterByAsync(
            location => location.WarehouseId == shipment.SourceWarehouseId,
            cancellationToken);
        var sourceLocationIds = sourceLocations.Select(location => location.Id).ToHashSet();
        var locationById = sourceLocations.ToDictionary(location => location.Id);
        var reservations = await reservationRepository.FilterByAsync(
            reservation => reservation.SourceType == "Shipment" && reservation.SourceId == shipment.Id &&
                           reservation.Status == StockReservationStatus.Active,
            cancellationToken);

        foreach (var item in items)
        {
            var reserved = reservations
                .Where(reservation => reservation.SourceItemId == item.Id)
                .Sum(reservation => reservation.Quantity - reservation.ReleasedQuantity);
            var missing = item.Quantity - reserved;
            if (missing > 0)
                await ReserveItemAsync(shipment, item, missing, false, sourceLocationIds, locationById, note, cancellationToken);
        }
    }

    private async Task ReserveItemAsync(
        Shipment shipment,
        ShipmentItem item,
        decimal quantity,
        bool allowPartial,
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
        if (available < quantity && !allowPartial)
            throw new CommandValidationException(
                $"Brak wystarczającej ilości dla kodu {item.Barcode}: brakuje {quantity - available:0.###}. Towar czeka na dostawę.");

        var remaining = quantity;
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
        await publisher.Publish(new InventoryChangedEvent(inventory.ProductId, inventory.LocationId,
            inventory.Quantity, inventory.ReservedQuantity, inventory.Id, 0), cancellationToken);

        await reservationRepository.AddAsync(new StockReservation
        {
            Id = Guid.NewGuid(),
            WarehouseId = shipment.SourceWarehouseId,
            LocationId = inventory.LocationId,
            ProductId = inventory.ProductId,
            InventoryId = inventory.Id,
            Quantity = quantity,
            ReleasedQuantity = 0,
            Status = StockReservationStatus.Active,
            SourceType = "Shipment",
            SourceId = shipment.Id,
            SourceItemId = item.Id,
            SourceNumber = shipment.Number,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = DateTime.UtcNow
        }, cancellationToken);

        await publisher.Publish(
            new StockMovementRecordedEvent(
                shipment.SourceWarehouseId,
                inventory.LocationId,
                inventory.ProductId,
                inventory.Id,
                StockMovementType.Reservation,
                quantityBefore,
                0,
                inventory.Quantity,
                reservedQuantityBefore,
                quantity,
                inventory.ReservedQuantity,
                "Shipment",
                shipment.Id,
                shipment.Number,
                note),
            cancellationToken);
    }
}
