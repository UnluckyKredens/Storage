using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class ReceiveShipmentCommandHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<ReceiveShipmentCommand, bool>
{
    public async ValueTask<bool> Handle(ReceiveShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FirstOrDefaultAsync(
            item => item.Id == command.ShipmentId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (shipment.Status != ShipmentStatus.InTransit)
            throw new CommandValidationException("Tylko wysyłka w drodze może zostać odebrana.");
        if (!warehouseContext.CanAccess(shipment.DestinationWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu docelowego.");

        var items = await shipmentItemRepository.FilterByAsync(
            item => item.ShipmentId == shipment.Id,
            cancellationToken);
        if (items.Count == 0)
            throw new CommandValidationException("Wysyłka nie ma pozycji.");

        var checkedItemIds = command.CheckedItemIds.ToHashSet();
        var missingItems = items
            .Where(item => !checkedItemIds.Contains(item.Id))
            .ToArray();
        if (missingItems.Length > 0)
            throw new CommandValidationException("Potwierdź wszystkie pozycje z checklisty przed przyjęciem wysyłki.");

        var inventoryChanges = new List<InventoryChangedEvent>();
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var destinationLocation = (await locationRepository.FilterByAsync(
                    location => location.WarehouseId == shipment.DestinationWarehouseId,
                    ct))
                .OrderBy(location => location.LocationCode == "REC-01" ? 0 : 1)
                .ThenBy(location => location.LocationCode)
                .FirstOrDefault()
                ?? throw new CommandValidationException("Magazyn docelowy nie ma lokalizacji przyjęcia.");

            foreach (var item in items)
            {
                var inventory = await inventoryRepository.FirstOrDefaultAsync(
                    candidate =>
                        candidate.ProductId == item.ProductId &&
                        candidate.LocationId == destinationLocation.Id,
                    ct);

                var quantityBefore = inventory?.Quantity ?? 0;
                var reservedQuantityBefore = inventory?.ReservedQuantity ?? 0;

                if (inventory is null)
                {
                    inventory = new Inventory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        LocationId = destinationLocation.Id,
                        Quantity = item.Quantity,
                        ReservedQuantity = 0
                    };
                    await inventoryRepository.AddAsync(inventory, ct);
                }
                else
                {
                    inventory.Quantity += item.Quantity;
                    await inventoryRepository.UpdateAsync(inventory, ct);
                }

                await publisher.Publish(
                    new StockMovementRecordedEvent(
                        destinationLocation.WarehouseId,
                        inventory.LocationId,
                        inventory.ProductId,
                        inventory.Id,
                        StockMovementType.TransferIn,
                        quantityBefore,
                        item.Quantity,
                        inventory.Quantity,
                        reservedQuantityBefore,
                        0,
                        inventory.ReservedQuantity,
                        "Shipment",
                        shipment.Id,
                        shipment.Number,
                        "Odbiór wysyłki."),
                    ct);

                inventoryChanges.Add(new InventoryChangedEvent(
                    inventory.ProductId,
                    inventory.LocationId,
                    inventory.Quantity,
                    inventory.ReservedQuantity,
                    inventory.Id,
                    item.Quantity));
            }

            shipment.Status = ShipmentStatus.Received;
            shipment.ReceivedByUserId = currentUser.UserId;
            shipment.ReceivedOnUtc = DateTime.UtcNow;
            await shipmentRepository.UpdateAsync(shipment, ct);
        }, cancellationToken);

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "Receive",
                nameof(Shipment),
                shipment.Id.ToString(),
                $"Odebrano wysyłkę {shipment.Number}."),
            cancellationToken);

        await publisher.Publish(new ShipmentReceivedEvent(
            shipment.Id,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            currentUser.UserId,
            shipment.Number,
            items.Count,
            command.Notes), cancellationToken);

        foreach (var change in inventoryChanges)
        {
            await publisher.Publish(change, cancellationToken);
        }

        return true;
    }
}
