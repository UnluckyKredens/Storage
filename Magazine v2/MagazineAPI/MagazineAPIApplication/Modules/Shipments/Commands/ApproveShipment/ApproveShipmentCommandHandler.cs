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

public sealed class ApproveShipmentCommandHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IShipmentReservationService shipmentReservationService,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<ApproveShipmentCommand, bool>
{
    public async ValueTask<bool> Handle(ApproveShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FirstOrDefaultAsync(
            item => item.Id == command.ShipmentId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (shipment.Status != ShipmentStatus.PendingApproval)
            throw new CommandValidationException("Tylko wysyłka oczekująca na akceptację może zostać zaakceptowana.");
        if (!warehouseContext.CanAccess(shipment.SourceWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu źródłowego.");

        var items = await shipmentItemRepository.FilterByAsync(
            item => item.ShipmentId == shipment.Id,
            cancellationToken);
        if (items.Count == 0)
            throw new CommandValidationException("Wysyłka nie ma pozycji.");

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await shipmentReservationService.ReserveMissingAsync(shipment, items, "Uzupełnienie rezerwacji przy akceptacji.", ct);
            shipment.Status = ShipmentStatus.Approved;
            shipment.ApprovedByUserId = currentUser.UserId;
            shipment.ApprovedOnUtc = DateTime.UtcNow;
            await shipmentRepository.UpdateAsync(shipment, ct);
        }, cancellationToken);

        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "Accept",
                nameof(Shipment),
                shipment.Id.ToString(),
                $"Zaakceptowano wysyłkę {shipment.Number}."),
            cancellationToken);

        await publisher.Publish(new ShipmentAcceptedEvent(
            shipment.Id,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            currentUser.UserId,
            shipment.Number,
            items.Count), cancellationToken);

        return true;
    }
}
