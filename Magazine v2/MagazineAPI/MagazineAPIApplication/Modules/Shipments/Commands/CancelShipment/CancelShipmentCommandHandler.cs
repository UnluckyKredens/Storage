using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class CancelShipmentCommandHandler(IRepository<Shipment> shipments,
    IRepository<StockReservation> reservations, IRepository<Inventory> inventories,
    IWarehouseContext warehouse, IAuditLogWriter audit, IPublisher publisher)
    : ICommandHandler<CancelShipmentCommand, bool>
{
    public async ValueTask<bool> Handle(CancelShipmentCommand command, CancellationToken ct)
    {
        var shipment = await shipments.FirstOrDefaultAsync(item => item.Id == command.ShipmentId, ct)
            ?? throw new ResourceNotFoundException("Nie znaleziono wysyłki.");
        if (!warehouse.CanAccess(shipment.SourceWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu źródłowego.");
        if (shipment.Status == ShipmentStatus.Cancelled) return true;
        if (shipment.Status != ShipmentStatus.PendingApproval)
            throw new CommandValidationException("Anulować można wyłącznie wysyłkę oczekującą na akceptację.");
        var active = await reservations.FilterByAsync(item => item.SourceType == "Shipment" &&
            item.SourceId == shipment.Id && item.Status == StockReservationStatus.Active, ct);
        foreach (var reservation in active)
        {
            var inventory = await inventories.FirstOrDefaultAsync(item => item.Id == reservation.InventoryId, ct)
                ?? throw new ResourceConflictException("Brakuje stanu dla rezerwacji.");
            var remaining = reservation.Quantity - reservation.ReleasedQuantity;
            if (inventory.ReservedQuantity < remaining || inventory.ProductId != reservation.ProductId || inventory.LocationId != reservation.LocationId)
                throw new ResourceConflictException("Rezerwacja jest niespójna ze stanem.");
            var before = inventory.ReservedQuantity;
            inventory.ReservedQuantity -= remaining;
            await inventories.UpdateAsync(inventory, ct);
            reservation.ReleasedQuantity = reservation.Quantity;
            reservation.Status = StockReservationStatus.Cancelled;
            reservation.ClosedOnUtc = DateTime.UtcNow;
            await reservations.UpdateAsync(reservation, ct);
            await publisher.Publish(new StockMovementRecordedEvent(reservation.WarehouseId, reservation.LocationId,
                reservation.ProductId, reservation.InventoryId, StockMovementType.ReservationRelease,
                inventory.Quantity, 0, inventory.Quantity, before, -remaining, inventory.ReservedQuantity,
                "Shipment", shipment.Id, shipment.Number, "Anulowanie wysyłki."), ct);
            await publisher.Publish(new InventoryChangedEvent(inventory.ProductId, inventory.LocationId,
                inventory.Quantity, inventory.ReservedQuantity, inventory.Id, 0), ct);
        }
        shipment.Status = ShipmentStatus.Cancelled;
        await shipments.UpdateAsync(shipment, ct);
        await audit.RecordAsync(new AuditLogRecord("Cancel", nameof(Shipment), shipment.Id.ToString(),
            $"Anulowano wysyłkę {shipment.Number}."), ct);
        await publisher.Publish(new ShipmentCancelledEvent(shipment.Id, shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId, warehouse.UserId, shipment.Number), ct);
        return true;
    }
}
