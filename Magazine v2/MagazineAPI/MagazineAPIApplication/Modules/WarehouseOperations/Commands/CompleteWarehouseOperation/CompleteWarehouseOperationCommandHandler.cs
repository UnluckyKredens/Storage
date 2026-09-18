using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.WarehouseOperations;

public sealed class CompleteWarehouseOperationCommandHandler(
    IRepository<WarehouseOperation> operationRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Inventory> inventoryRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IDocumentNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    IStockMovementWriter stockMovementWriter,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<CompleteWarehouseOperationCommand, Guid>
{
    public async ValueTask<Guid> Handle(CompleteWarehouseOperationCommand command, CancellationToken cancellationToken)
    {
        if (command.Items.Count == 0)
            throw new CommandValidationException("Dodaj co najmniej jedną pozycję dokumentu.");

        var locationIds = command.Items
            .SelectMany(item => new[] { item.SourceLocationId, item.DestinationLocationId })
            .OfType<Guid>()
            .ToHashSet();
        var locations = (await locationRepository.AllAsync(cancellationToken))
            .Where(location => locationIds.Contains(location.LocationId))
            .ToDictionary(location => location.LocationId);
        var operationWarehouseId = ResolveOperationWarehouseId(command, locations);

        if (!warehouseContext.CanAccess(operationWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu dokumentu.");

        foreach (var item in command.Items)
        {
            if (await productRepository.FirstOrDefaultAsync(
                    product => product.ProductId == item.ProductId && product.IsActive,
                    cancellationToken) is null)
                throw new CommandValidationException("Dokument zawiera nieaktywny albo nieistniejący produkt.");
            ValidateItem(command.Type, item, locations);
        }

        var now = DateTime.UtcNow;
        var number = await numberGenerator.GenerateAsync(
            operationWarehouseId,
            DocumentType(command.Type),
            now,
            cancellationToken);

        var operation = new WarehouseOperation
        {
            WarehouseOperationId = Guid.NewGuid(),
            Number = number,
            Type = command.Type,
            Status = WarehouseOperationStatus.Completed,
            WarehouseId = operationWarehouseId,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = now,
            CompletedByUserId = currentUser.UserId,
            CompletedOnUtc = now,
            Notes = command.Notes,
            Items = command.Items.Select(item => new WarehouseOperationItem
            {
                WarehouseOperationItemId = Guid.NewGuid(),
                ProductId = item.ProductId,
                SourceLocationId = item.SourceLocationId,
                DestinationLocationId = item.DestinationLocationId,
                Quantity = item.Quantity,
                TargetQuantity = item.TargetQuantity
            }).ToList()
        };

        var inventoryChanges = new List<InventoryChangedEvent>();
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await operationRepository.AddAsync(operation, ct);

            foreach (var item in command.Items)
            {
                await ApplyItemAsync(command.Type, operation, item, locations, inventoryChanges, ct);
            }

            await auditLogWriter.RecordAsync(
                new AuditLogRecord(
                    "Complete",
                    nameof(WarehouseOperation),
                    operation.WarehouseOperationId.ToString(),
                    $"Zaksięgowano dokument {operation.Number}."),
                ct);
        }, cancellationToken);

        foreach (var change in inventoryChanges)
        {
            await publisher.Publish(change, cancellationToken);
        }

        return operation.WarehouseOperationId;
    }

    private async Task ApplyItemAsync(
        WarehouseOperationType type,
        WarehouseOperation operation,
        CompleteWarehouseOperationItem item,
        IReadOnlyDictionary<Guid, Location> locations,
        List<InventoryChangedEvent> inventoryChanges,
        CancellationToken cancellationToken)
    {
        switch (type)
        {
            case WarehouseOperationType.InternalReceipt:
                await ApplyReceiptAsync(operation, item, locations[item.DestinationLocationId!.Value], inventoryChanges, cancellationToken);
                break;
            case WarehouseOperationType.InternalIssue:
                await ApplyIssueAsync(operation, item, locations[item.SourceLocationId!.Value], inventoryChanges, cancellationToken);
                break;
            case WarehouseOperationType.InternalTransfer:
                await ApplyTransferAsync(
                    operation,
                    item,
                    locations[item.SourceLocationId!.Value],
                    locations[item.DestinationLocationId!.Value],
                    inventoryChanges,
                    cancellationToken);
                break;
            case WarehouseOperationType.Correction:
            case WarehouseOperationType.InventoryCount:
                await ApplyTargetQuantityAsync(operation, type, item, locations[item.SourceLocationId!.Value], inventoryChanges, cancellationToken);
                break;
            default:
                throw new CommandValidationException("Nieobsługiwany typ dokumentu magazynowego.");
        }
    }

    private async Task ApplyReceiptAsync(
        WarehouseOperation operation,
        CompleteWarehouseOperationItem item,
        Location destinationLocation,
        List<InventoryChangedEvent> inventoryChanges,
        CancellationToken cancellationToken)
    {
        var inventory = await GetOrCreateInventoryAsync(item.ProductId, destinationLocation.LocationId, cancellationToken);
        var quantityBefore = inventory.Quantity;
        var reservedBefore = inventory.ReservedQuantity;
        inventory.Quantity += item.Quantity;
        await inventoryRepository.UpdateAsync(inventory, cancellationToken);
        await RecordMovementAsync(operation, inventory, destinationLocation, StockMovementType.InternalReceipt, quantityBefore, item.Quantity, reservedBefore, 0, "Przyjęcie wewnętrzne PW.", cancellationToken);
        inventoryChanges.Add(ToInventoryChangedEvent(inventory, item.Quantity));
    }

    private async Task ApplyIssueAsync(
        WarehouseOperation operation,
        CompleteWarehouseOperationItem item,
        Location sourceLocation,
        List<InventoryChangedEvent> inventoryChanges,
        CancellationToken cancellationToken)
    {
        var inventory = await GetExistingInventoryAsync(item.ProductId, sourceLocation.LocationId, cancellationToken);
        if (inventory.AvailableQuantity < item.Quantity)
            throw new CommandValidationException("Brak wystarczającej dostępnej ilości dla RW.");
        var quantityBefore = inventory.Quantity;
        var reservedBefore = inventory.ReservedQuantity;
        inventory.Quantity -= item.Quantity;
        await inventoryRepository.UpdateAsync(inventory, cancellationToken);
        await RecordMovementAsync(operation, inventory, sourceLocation, StockMovementType.InternalIssue, quantityBefore, -item.Quantity, reservedBefore, 0, "Rozchód wewnętrzny RW.", cancellationToken);
        inventoryChanges.Add(ToInventoryChangedEvent(inventory, -item.Quantity));
    }

    private async Task ApplyTransferAsync(
        WarehouseOperation operation,
        CompleteWarehouseOperationItem item,
        Location sourceLocation,
        Location destinationLocation,
        List<InventoryChangedEvent> inventoryChanges,
        CancellationToken cancellationToken)
    {
        var sourceInventory = await GetExistingInventoryAsync(item.ProductId, sourceLocation.LocationId, cancellationToken);
        if (sourceInventory.AvailableQuantity < item.Quantity)
            throw new CommandValidationException("Brak wystarczającej dostępnej ilości dla MM.");

        var sourceQuantityBefore = sourceInventory.Quantity;
        var sourceReservedBefore = sourceInventory.ReservedQuantity;
        sourceInventory.Quantity -= item.Quantity;
        await inventoryRepository.UpdateAsync(sourceInventory, cancellationToken);
        await RecordMovementAsync(operation, sourceInventory, sourceLocation, StockMovementType.InternalTransferOut, sourceQuantityBefore, -item.Quantity, sourceReservedBefore, 0, "Wydanie MM z lokalizacji źródłowej.", cancellationToken);
        inventoryChanges.Add(ToInventoryChangedEvent(sourceInventory, -item.Quantity));

        var destinationInventory = await GetOrCreateInventoryAsync(item.ProductId, destinationLocation.LocationId, cancellationToken);
        var destinationQuantityBefore = destinationInventory.Quantity;
        var destinationReservedBefore = destinationInventory.ReservedQuantity;
        destinationInventory.Quantity += item.Quantity;
        await inventoryRepository.UpdateAsync(destinationInventory, cancellationToken);
        await RecordMovementAsync(operation, destinationInventory, destinationLocation, StockMovementType.InternalTransferIn, destinationQuantityBefore, item.Quantity, destinationReservedBefore, 0, "Przyjęcie MM do lokalizacji docelowej.", cancellationToken);
        inventoryChanges.Add(ToInventoryChangedEvent(destinationInventory, item.Quantity));
    }

    private async Task ApplyTargetQuantityAsync(
        WarehouseOperation operation,
        WarehouseOperationType operationType,
        CompleteWarehouseOperationItem item,
        Location location,
        List<InventoryChangedEvent> inventoryChanges,
        CancellationToken cancellationToken)
    {
        var inventory = await GetOrCreateInventoryAsync(item.ProductId, location.LocationId, cancellationToken);
        var targetQuantity = item.TargetQuantity!.Value;
        if (targetQuantity < inventory.ReservedQuantity)
            throw new CommandValidationException("Docelowa ilość nie może być niższa niż aktywne rezerwacje.");

        var quantityBefore = inventory.Quantity;
        var reservedBefore = inventory.ReservedQuantity;
        var quantityChange = targetQuantity - inventory.Quantity;
        inventory.Quantity = targetQuantity;
        await inventoryRepository.UpdateAsync(inventory, cancellationToken);
        await RecordMovementAsync(
            operation,
            inventory,
            location,
            operationType == WarehouseOperationType.InventoryCount ? StockMovementType.InventoryCount : StockMovementType.Correction,
            quantityBefore,
            quantityChange,
            reservedBefore,
            0,
            operationType == WarehouseOperationType.InventoryCount ? "Inwentaryzacja stanu." : "Korekta stanu.",
            cancellationToken);
        inventoryChanges.Add(ToInventoryChangedEvent(inventory, quantityChange));
    }

    private async Task<Inventory> GetExistingInventoryAsync(Guid productId, Guid locationId, CancellationToken cancellationToken)
    {
        return await inventoryRepository.FirstOrDefaultAsync(
            inventory => inventory.ProductId == productId && inventory.LocationId == locationId,
            cancellationToken)
            ?? throw new CommandValidationException("Nie znaleziono stanu magazynowego dla wskazanej pozycji.");
    }

    private async Task<Inventory> GetOrCreateInventoryAsync(Guid productId, Guid locationId, CancellationToken cancellationToken)
    {
        var inventory = await inventoryRepository.FirstOrDefaultAsync(
            candidate => candidate.ProductId == productId && candidate.LocationId == locationId,
            cancellationToken);
        if (inventory is not null) return inventory;

        inventory = new Inventory
        {
            InventoryId = Guid.NewGuid(),
            ProductId = productId,
            LocationId = locationId,
            Quantity = 0,
            ReservedQuantity = 0
        };
        await inventoryRepository.AddAsync(inventory, cancellationToken);
        return inventory;
    }

    private async Task RecordMovementAsync(
        WarehouseOperation operation,
        Inventory inventory,
        Location location,
        StockMovementType type,
        decimal quantityBefore,
        decimal quantityChange,
        decimal reservedBefore,
        decimal reservedChange,
        string notes,
        CancellationToken cancellationToken)
    {
        await stockMovementWriter.RecordAsync(
            new StockMovementRecord(
                location.WarehouseId,
                inventory.LocationId,
                inventory.ProductId,
                inventory.InventoryId,
                type,
                quantityBefore,
                quantityChange,
                inventory.Quantity,
                reservedBefore,
                reservedChange,
                inventory.ReservedQuantity,
                "WarehouseOperation",
                operation.WarehouseOperationId,
                operation.Number,
                notes),
            cancellationToken);
    }

    private static InventoryChangedEvent ToInventoryChangedEvent(Inventory inventory, decimal quantityChange)
    {
        return new InventoryChangedEvent(
            inventory.ProductId,
            inventory.LocationId,
            inventory.Quantity,
            inventory.ReservedQuantity,
            inventory.InventoryId,
            quantityChange);
    }

    private static Guid ResolveOperationWarehouseId(
        CompleteWarehouseOperationCommand command,
        IReadOnlyDictionary<Guid, Location> locations)
    {
        var firstItem = command.Items.First();
        var locationId = command.Type switch
        {
            WarehouseOperationType.InternalReceipt => firstItem.DestinationLocationId,
            WarehouseOperationType.InternalIssue => firstItem.SourceLocationId,
            WarehouseOperationType.InternalTransfer => firstItem.SourceLocationId,
            WarehouseOperationType.Correction => firstItem.SourceLocationId,
            WarehouseOperationType.InventoryCount => firstItem.SourceLocationId,
            _ => null
        };

        if (locationId is { } id && locations.TryGetValue(id, out var location))
            return location.WarehouseId;
        return command.WarehouseId
            ?? throw new CommandValidationException("Nie można ustalić magazynu dokumentu.");
    }

    private static void ValidateItem(
        WarehouseOperationType type,
        CompleteWarehouseOperationItem item,
        IReadOnlyDictionary<Guid, Location> locations)
    {
        if (item.Quantity < 0)
            throw new CommandValidationException("Ilość nie może być ujemna.");
        if (item.SourceLocationId is { } sourceId && !locations.ContainsKey(sourceId))
            throw new CommandValidationException("Lokalizacja źródłowa nie istnieje.");
        if (item.DestinationLocationId is { } destinationId && !locations.ContainsKey(destinationId))
            throw new CommandValidationException("Lokalizacja docelowa nie istnieje.");

        switch (type)
        {
            case WarehouseOperationType.InternalReceipt:
                if (item.DestinationLocationId is null || item.Quantity <= 0)
                    throw new CommandValidationException("PW wymaga lokalizacji docelowej i ilości większej od zera.");
                break;
            case WarehouseOperationType.InternalIssue:
                if (item.SourceLocationId is null || item.Quantity <= 0)
                    throw new CommandValidationException("RW wymaga lokalizacji źródłowej i ilości większej od zera.");
                break;
            case WarehouseOperationType.InternalTransfer:
                if (item.SourceLocationId is null || item.DestinationLocationId is null || item.Quantity <= 0)
                    throw new CommandValidationException("MM wymaga obu lokalizacji i ilości większej od zera.");
                if (item.SourceLocationId == item.DestinationLocationId)
                    throw new CommandValidationException("Lokalizacja źródłowa i docelowa MM muszą być różne.");
                break;
            case WarehouseOperationType.Correction:
            case WarehouseOperationType.InventoryCount:
                if (item.SourceLocationId is null || item.TargetQuantity is null || item.TargetQuantity < 0)
                    throw new CommandValidationException("Korekta i inwentaryzacja wymagają lokalizacji oraz nieujemnej ilości docelowej.");
                break;
            default:
                throw new CommandValidationException("Nieobsługiwany typ dokumentu magazynowego.");
        }
    }

    private static string DocumentType(WarehouseOperationType type) => type switch
    {
        WarehouseOperationType.InternalReceipt => "PW",
        WarehouseOperationType.InternalIssue => "RW",
        WarehouseOperationType.InternalTransfer => "MM",
        WarehouseOperationType.Correction => "KOR",
        WarehouseOperationType.InventoryCount => "INV",
        _ => "MAG"
    };
}
