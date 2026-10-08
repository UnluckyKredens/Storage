using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed class ReceivePurchaseOrderCommandHandler(
    IRepository<PurchaseOrder> orderRepository,
    IRepository<PurchaseOrderItem> orderItemRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IStockMovementWriter stockMovementWriter,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<ReceivePurchaseOrderCommand, bool>
{
    public async ValueTask<bool> Handle(ReceivePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FirstOrDefaultAsync(
            item => item.PurchaseOrderId == command.PurchaseOrderId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Zamówienie zewnętrzne nie zostało znalezione.");

        if (order.Status != PurchaseOrderStatus.Approved)
            throw new CommandValidationException("Tylko zaakceptowane zamówienie może zostać przyjęte.");
        if (!warehouseContext.CanAccess(order.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu zamówienia.");

        var items = await orderItemRepository.FilterByAsync(
            item => item.PurchaseOrderId == order.PurchaseOrderId,
            cancellationToken);
        if (items.Count == 0)
            throw new CommandValidationException("Zamówienie nie ma pozycji.");

        var checkedItemIds = command.CheckedItemIds.ToHashSet();
        var missingItems = items.Where(item => !checkedItemIds.Contains(item.PurchaseOrderItemId)).ToArray();
        if (missingItems.Length > 0)
            throw new CommandValidationException("Potwierdź wszystkie pozycje z checklisty przed przyjęciem zamówienia.");

        var inventoryChanges = new List<InventoryChangedEvent>();
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var destinationLocation = (await locationRepository.FilterByAsync(
                    location => location.WarehouseId == order.WarehouseId,
                    ct))
                .OrderBy(location => location.LocationCode == "REC-01" ? 0 : 1)
                .ThenBy(location => location.LocationCode)
                .FirstOrDefault()
                ?? throw new CommandValidationException("Magazyn nie ma lokalizacji przyjęcia.");

            foreach (var item in items)
            {
                var inventory = await inventoryRepository.FirstOrDefaultAsync(
                    candidate =>
                        candidate.ProductId == item.ProductId &&
                        candidate.LocationId == destinationLocation.LocationId,
                    ct);

                var quantityBefore = inventory?.Quantity ?? 0;
                var reservedBefore = inventory?.ReservedQuantity ?? 0;

                if (inventory is null)
                {
                    inventory = new Inventory
                    {
                        InventoryId = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        LocationId = destinationLocation.LocationId,
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

                await stockMovementWriter.RecordAsync(
                    new StockMovementRecord(
                        destinationLocation.WarehouseId,
                        inventory.LocationId,
                        inventory.ProductId,
                        inventory.InventoryId,
                        StockMovementType.ExternalPurchaseReceipt,
                        quantityBefore,
                        item.Quantity,
                        inventory.Quantity,
                        reservedBefore,
                        0,
                        inventory.ReservedQuantity,
                        "PurchaseOrder",
                        order.PurchaseOrderId,
                        order.Number,
                        $"Przyjęcie zamówienia zewnętrznego, faktura {order.InvoiceNumber}."),
                    ct);

                inventoryChanges.Add(new InventoryChangedEvent(
                    inventory.ProductId,
                    inventory.LocationId,
                    inventory.Quantity,
                    inventory.ReservedQuantity,
                    inventory.InventoryId,
                    item.Quantity));
            }

            order.Status = PurchaseOrderStatus.Received;
            order.ReceivedByUserId = currentUser.UserId;
            order.ReceivedOnUtc = DateTime.UtcNow;
            order.Notes = command.Notes?.Trim() ?? order.Notes;
            await orderRepository.UpdateAsync(order, ct);
        }, cancellationToken);

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "Receive",
                nameof(PurchaseOrder),
                order.PurchaseOrderId.ToString(),
                $"Przyjęto zamówienie {order.Number} na stan."),
            cancellationToken);

        foreach (var change in inventoryChanges)
        {
            await publisher.Publish(change, cancellationToken);
        }

        return true;
    }
}
