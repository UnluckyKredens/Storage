using System.Text.Json;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed class SaveInventoryCommandHandler(
    IRepository<Inventory> inventoryRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext,
    IStockMovementWriter stockMovementWriter,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<SaveInventoryCommand, Guid>
{
    public async ValueTask<Guid> Handle(SaveInventoryCommand command, CancellationToken cancellationToken)
    {
        if (command.Quantity < 0 || command.ReservedQuantity < 0 || command.ReservedQuantity > command.Quantity)
            throw new CommandValidationException("Ilość i rezerwacja muszą być nieujemne, a rezerwacja nie może przekraczać ilości.");
        if (await productRepository.FirstOrDefaultAsync(x => x.ProductId == command.ProductId, cancellationToken) is null)
            throw new CommandValidationException("Produkt nie istnieje.");
        var location = await locationRepository.FirstOrDefaultAsync(
            x => x.LocationId == command.LocationId, cancellationToken)
            ?? throw new CommandValidationException("Lokalizacja nie istnieje.");
        if (!warehouseContext.CanAccess(location.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");

        var samePair = await inventoryRepository.FirstOrDefaultAsync(
            x => x.ProductId == command.ProductId && x.LocationId == command.LocationId, cancellationToken);
        if (samePair is not null && samePair.InventoryId != command.Id)
            throw new ResourceConflictException("Stan tego produktu w tej lokalizacji już istnieje.");

        Inventory item;
        decimal quantityChange;
        var action = command.Id is null ? "Create" : "Update";
        object? beforeSnapshot = null;
        var movements = new List<StockMovementRecord>();

        if (command.Id is null)
        {
            item = new Inventory { InventoryId = Guid.NewGuid() };
            Assign(item, command);
            await inventoryRepository.AddAsync(item, cancellationToken);
            quantityChange = command.Quantity;
            movements.Add(new StockMovementRecord(
                location.WarehouseId,
                item.LocationId,
                item.ProductId,
                item.InventoryId,
                StockMovementType.ManualAdjustment,
                0,
                item.Quantity,
                item.Quantity,
                0,
                item.ReservedQuantity,
                item.ReservedQuantity,
                "Inventory",
                item.InventoryId,
                null,
                "Utworzenie stanu magazynowego."));
        }
        else
        {
            item = await inventoryRepository.FirstOrDefaultAsync(x => x.InventoryId == command.Id, cancellationToken)
                ?? throw new ResourceNotFoundException("Stan magazynowy nie został znaleziony.");
            var currentLocation = await locationRepository.FirstOrDefaultAsync(
                x => x.LocationId == item.LocationId, cancellationToken);
            if (currentLocation is not null && !warehouseContext.CanAccess(currentLocation.WarehouseId))
                throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");

            beforeSnapshot = Snapshot(item);
            quantityChange = command.Quantity - item.Quantity;

            if (item.ProductId != command.ProductId || item.LocationId != command.LocationId)
            {
                if (currentLocation is not null)
                {
                    movements.Add(new StockMovementRecord(
                        currentLocation.WarehouseId,
                        item.LocationId,
                        item.ProductId,
                        item.InventoryId,
                        StockMovementType.ManualAdjustment,
                        item.Quantity,
                        -item.Quantity,
                        0,
                        item.ReservedQuantity,
                        -item.ReservedQuantity,
                        0,
                        "Inventory",
                        item.InventoryId,
                        null,
                        "Przeniesienie stanu z poprzedniego produktu lub lokalizacji."));
                }

                movements.Add(new StockMovementRecord(
                    location.WarehouseId,
                    command.LocationId,
                    command.ProductId,
                    item.InventoryId,
                    StockMovementType.ManualAdjustment,
                    0,
                    command.Quantity,
                    command.Quantity,
                    0,
                    command.ReservedQuantity,
                    command.ReservedQuantity,
                    "Inventory",
                    item.InventoryId,
                    null,
                    "Przeniesienie stanu do nowego produktu lub lokalizacji."));
            }
            else if (command.Quantity != item.Quantity || command.ReservedQuantity != item.ReservedQuantity)
            {
                movements.Add(new StockMovementRecord(
                    location.WarehouseId,
                    item.LocationId,
                    item.ProductId,
                    item.InventoryId,
                    StockMovementType.ManualAdjustment,
                    item.Quantity,
                    command.Quantity - item.Quantity,
                    command.Quantity,
                    item.ReservedQuantity,
                    command.ReservedQuantity - item.ReservedQuantity,
                    command.ReservedQuantity,
                    "Inventory",
                    item.InventoryId,
                    null,
                    "Ręczna korekta stanu magazynowego."));
            }

            Assign(item, command);
            await inventoryRepository.UpdateAsync(item, cancellationToken);
        }

        foreach (var movement in movements)
        {
            await stockMovementWriter.RecordAsync(movement, cancellationToken);
        }

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                action,
                nameof(Inventory),
                item.InventoryId.ToString(),
                command.Id is null ? "Utworzono stan magazynowy." : "Zaktualizowano stan magazynowy.",
                beforeSnapshot is null ? null : JsonSerializer.Serialize(beforeSnapshot),
                JsonSerializer.Serialize(Snapshot(item))),
            cancellationToken);

        await publisher.Publish(
            new InventoryChangedEvent(
                command.ProductId, command.LocationId, command.Quantity, command.ReservedQuantity,
                item.InventoryId, quantityChange),
            cancellationToken);

        return item.InventoryId;
    }

    private static void Assign(Inventory item, SaveInventoryCommand command)
    {
        item.ProductId = command.ProductId;
        item.LocationId = command.LocationId;
        item.Quantity = command.Quantity;
        item.ReservedQuantity = command.ReservedQuantity;
    }

    private static object Snapshot(Inventory item) => new
    {
        item.InventoryId,
        item.ProductId,
        item.LocationId,
        item.Quantity,
        item.ReservedQuantity
    };
}
