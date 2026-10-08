using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class ApproveShipmentCommandHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<Location> locationRepository,
    IRepository<StockReservation> reservationRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IStockMovementWriter stockMovementWriter,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<ApproveShipmentCommand, bool>
{
    public async ValueTask<bool> Handle(ApproveShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FirstOrDefaultAsync(
            item => item.ShipmentId == command.ShipmentId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (shipment.Status != ShipmentStatus.PendingApproval)
            throw new CommandValidationException("Tylko wysyłka oczekująca na akceptację może zostać wysłana.");
        if (!warehouseContext.CanAccess(shipment.SourceWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu źródłowego.");

        var items = await shipmentItemRepository.FilterByAsync(
            item => item.ShipmentId == shipment.ShipmentId,
            cancellationToken);
        if (items.Count == 0)
            throw new CommandValidationException("Wysyłka nie ma pozycji.");

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var activeReservations = await reservationRepository.FilterByAsync(
                reservation =>
                    reservation.SourceType == "Shipment" &&
                    reservation.SourceId == shipment.ShipmentId &&
                    reservation.Status == StockReservationStatus.Active,
                ct);

            if (activeReservations.Count > 0)
            {
                foreach (var item in items)
                {
                    var reserved = activeReservations
                        .Where(reservation => reservation.SourceItemId == item.ShipmentItemId)
                        .Sum(reservation => reservation.Quantity - reservation.ReleasedQuantity);
                    if (reserved < item.Quantity)
                        throw new CommandValidationException($"Rezerwacja dla kodu {item.Barcode} nie pokrywa ilości wysyłki.");
                }

                foreach (var reservation in activeReservations)
                {
                    var take = reservation.Quantity - reservation.ReleasedQuantity;
                    if (take <= 0) continue;
                    var inventory = await inventoryRepository.FirstOrDefaultAsync(
                        candidate => candidate.InventoryId == reservation.InventoryId,
                        ct)
                        ?? throw new CommandValidationException("Nie znaleziono stanu powiązanego z rezerwacją.");
                    if (inventory.Quantity < take || inventory.ReservedQuantity < take)
                        throw new CommandValidationException("Stan magazynowy jest niższy niż aktywna rezerwacja.");

                    var quantityBefore = inventory.Quantity;
                    var reservedQuantityBefore = inventory.ReservedQuantity;
                    inventory.Quantity -= take;
                    inventory.ReservedQuantity -= take;
                    await inventoryRepository.UpdateAsync(inventory, ct);

                    reservation.ReleasedQuantity = reservation.Quantity;
                    reservation.Status = StockReservationStatus.Consumed;
                    reservation.ClosedOnUtc = DateTime.UtcNow;
                    await reservationRepository.UpdateAsync(reservation, ct);

                    await stockMovementWriter.RecordAsync(
                        new StockMovementRecord(
                            reservation.WarehouseId,
                            inventory.LocationId,
                            inventory.ProductId,
                            inventory.InventoryId,
                            StockMovementType.TransferOut,
                            quantityBefore,
                            -take,
                            inventory.Quantity,
                            reservedQuantityBefore,
                            -take,
                            inventory.ReservedQuantity,
                            "Shipment",
                            shipment.ShipmentId,
                            shipment.Number,
                            "Wysłanie towaru z rezerwacji."),
                        ct);
                }
            }
            else
            {
                await ConsumeLegacyAvailableStockAsync(shipment, items, ct);
            }

            shipment.Status = ShipmentStatus.Sent;
            shipment.ApprovedByUserId = currentUser.UserId;
            shipment.ApprovedOnUtc = DateTime.UtcNow;
            await shipmentRepository.UpdateAsync(shipment, ct);
        }, cancellationToken);

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "Approve",
                nameof(Shipment),
                shipment.ShipmentId.ToString(),
                $"Wysłano wysyłkę {shipment.Number}."),
            cancellationToken);

        await publisher.Publish(new ShipmentApprovedEvent(
            shipment.ShipmentId,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            currentUser.UserId,
            shipment.Number,
            items.Count), cancellationToken);

        return true;
    }

    private async Task ConsumeLegacyAvailableStockAsync(
        Shipment shipment,
        IReadOnlyList<ShipmentItem> items,
        CancellationToken cancellationToken)
    {
        var sourceLocations = await locationRepository.FilterByAsync(
            location => location.WarehouseId == shipment.SourceWarehouseId,
            cancellationToken);
        var sourceLocationIds = sourceLocations
            .OrderBy(location => location.LocationCode)
            .Select(location => location.LocationId)
            .ToHashSet();

        foreach (var item in items)
        {
            var inventories = (await inventoryRepository.FilterByAsync(
                    inventory => inventory.ProductId == item.ProductId,
                    cancellationToken))
                .Where(inventory => sourceLocationIds.Contains(inventory.LocationId))
                .OrderBy(inventory => sourceLocations.First(location => location.LocationId == inventory.LocationId).LocationCode)
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

                var quantityBefore = inventory.Quantity;
                var reservedQuantityBefore = inventory.ReservedQuantity;
                inventory.Quantity -= take;
                remaining -= take;
                await inventoryRepository.UpdateAsync(inventory, cancellationToken);

                var movementLocation = sourceLocations.First(location => location.LocationId == inventory.LocationId);
                await stockMovementWriter.RecordAsync(
                    new StockMovementRecord(
                        movementLocation.WarehouseId,
                        inventory.LocationId,
                        inventory.ProductId,
                        inventory.InventoryId,
                        StockMovementType.TransferOut,
                        quantityBefore,
                        -take,
                        inventory.Quantity,
                        reservedQuantityBefore,
                        0,
                        inventory.ReservedQuantity,
                        "Shipment",
                        shipment.ShipmentId,
                        shipment.Number,
                        "Wysłanie starszej wysyłki bez rezerwacji."),
                    cancellationToken);
            }
        }
    }
}
