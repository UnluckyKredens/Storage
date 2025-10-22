using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Mediator;
using MagazineAPIEvent.Request;

namespace MagazineAPI.Services;

public sealed class StorefrontOrderService(AppDbContext db, IAuditLogWriter audit, IPublisher publisher)
{
    public IQueryable<StorefrontOrder> OrdersWithAllocations() => db.StorefrontOrders
        .Include(order => order.Items).ThenInclude(item => item.Allocations);

    // The caller holds the operational transaction and has checked access/status.
    public async Task ReleaseAsync(StorefrontOrder order, string reason, CancellationToken ct)
    {
        if (order.Status is StorefrontOrderStatus.Cancelled or StorefrontOrderStatus.Completed) return;
        foreach (var allocation in order.Items.SelectMany(item => item.Allocations))
        {
            var quantity = allocation.Quantity - allocation.ConsumedQuantity;
            if (quantity <= 0) continue;
            var inventory = await db.Inventory.SingleAsync(item => item.Id == allocation.InventoryId, ct);
            if (inventory.ReservedQuantity < quantity || inventory.ProductId != allocation.ProductId ||
                inventory.LocationId != allocation.LocationId)
                throw new ResourceConflictException("Rezerwacja zamówienia wymaga uzgodnienia ze stanem.");
            var reservedBefore = inventory.ReservedQuantity;
            inventory.ReservedQuantity -= quantity;
            await publisher.Publish(new StockMovementRecordedEvent(allocation.WarehouseId, allocation.LocationId,
                allocation.ProductId, allocation.InventoryId, StockMovementType.ReservationRelease,
                inventory.Quantity, 0, inventory.Quantity, reservedBefore, -quantity, inventory.ReservedQuantity,
                "StorefrontOrder", order.Id, order.Number, reason), ct);
            await publisher.Publish(new InventoryChangedEvent(inventory.ProductId, inventory.LocationId,
                inventory.Quantity, inventory.ReservedQuantity, inventory.Id, 0), ct);
        }
        order.Status = StorefrontOrderStatus.Cancelled;
        await audit.RecordAsync(new AuditLogRecord("Cancel", nameof(StorefrontOrder),
            order.Id.ToString(), reason), ct);
        await db.SaveChangesAsync(ct);
    }
}
