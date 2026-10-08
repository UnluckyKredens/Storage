using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class MarkShipmentInTransitCommandHandler(
    IRepository<Shipment> shipmentRepository,
    IWarehouseContext warehouseContext,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<MarkShipmentInTransitCommand, bool>
{
    public async ValueTask<bool> Handle(MarkShipmentInTransitCommand command, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FirstOrDefaultAsync(
            candidate => candidate.ShipmentId == command.ShipmentId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (shipment.Status != ShipmentStatus.Sent)
            throw new CommandValidationException("Tylko wysłana wysyłka może zostać oznaczona jako w drodze.");
        if (!warehouseContext.CanAccess(shipment.SourceWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu źródłowego.");

        shipment.Status = ShipmentStatus.InTransit;
        await shipmentRepository.UpdateAsync(shipment, cancellationToken);

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "MarkInTransit",
                nameof(Shipment),
                shipment.ShipmentId.ToString(),
                $"Oznaczono wysyłkę {shipment.Number} jako w drodze."),
            cancellationToken);

        await publisher.Publish(new ShipmentInTransitEvent(
            shipment.ShipmentId,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            warehouseContext.UserId,
            shipment.Number), cancellationToken);

        return true;
    }
}
