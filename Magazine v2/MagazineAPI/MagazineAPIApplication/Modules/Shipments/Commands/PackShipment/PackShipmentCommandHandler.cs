using MagazineAPIApplication.Common;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class PackShipmentCommandHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<PackShipmentCommand, bool>
{
    public async ValueTask<bool> Handle(PackShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FirstOrDefaultAsync(
            candidate => candidate.Id == command.ShipmentId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (shipment.Status != ShipmentStatus.Approved)
            throw new CommandValidationException("Tylko zaakceptowana wysyłka może zostać spakowana.");
        if (!warehouseContext.CanAccess(shipment.SourceWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu źródłowego.");

        var itemCount = (await shipmentItemRepository.FilterByAsync(
            item => item.ShipmentId == shipment.Id,
            cancellationToken)).Count;

        shipment.Status = ShipmentStatus.Packed;
        shipment.PackedByUserId = currentUser.UserId;
        shipment.PackedOnUtc = DateTime.UtcNow;
        await shipmentRepository.UpdateAsync(shipment, cancellationToken);

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "Pack",
                nameof(Shipment),
                shipment.Id.ToString(),
                $"Spakowano wysyłkę {shipment.Number}."),
            cancellationToken);

        await publisher.Publish(new ShipmentPackedEvent(
            shipment.Id,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            warehouseContext.UserId,
            shipment.Number,
            itemCount), cancellationToken);

        return true;
    }
}
