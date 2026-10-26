using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class GetShipmentHistoryQueryHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentHistory> shipmentHistoryRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<User> userRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetShipmentHistoryQuery, IReadOnlyList<ShipmentHistoryView>>
{
    public async ValueTask<IReadOnlyList<ShipmentHistoryView>> Handle(
        GetShipmentHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FirstOrDefaultAsync(
            candidate => candidate.Id == query.ShipmentId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (!warehouseContext.CanAccess(shipment.SourceWarehouseId) &&
            !warehouseContext.CanAccess(shipment.DestinationWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do historii wysyłki.");

        var histories = await shipmentHistoryRepository.FilterByAsync(
            history => history.ShipmentId == query.ShipmentId,
            cancellationToken);
        var warehouses = (await warehouseRepository.AllAsync(cancellationToken))
            .ToDictionary(warehouse => warehouse.Id);
        var users = (await userRepository.AllAsync(cancellationToken))
            .ToDictionary(user => user.Id);

        return histories
            .OrderBy(history => history.CreatedOnUtc)
            .Select(history =>
            {
                var source = warehouses.GetValueOrDefault(history.SourceWarehouseId);
                var destination = warehouses.GetValueOrDefault(history.DestinationWarehouseId);
                var user = history.UserId is { } userId
                    ? users.GetValueOrDefault(userId)
                    : null;

                return new ShipmentHistoryView(
                    history.Id,
                    history.EventType,
                    DisplayEvent(history.EventType),
                    history.SourceWarehouseId,
                    source?.Name ?? string.Empty,
                    history.DestinationWarehouseId,
                    destination?.Name ?? string.Empty,
                    history.UserId,
                    user is null ? null : $"{user.FirstName} {user.LastName}",
                    history.CreatedOnUtc,
                    history.Details);
            })
            .ToArray();
    }

    private static string DisplayEvent(string eventType) => eventType switch
    {
        "created" => "Utworzono wysyłkę",
        "requested" => "Złożono prośbę",
        "accepted" => "Zaakceptowano",
        "packed" => "Spakowano",
        // "approved" to historyczna nazwa zdarzenia wysłania towaru (zachowana dla istniejących danych).
        "approved" => "Wysłano",
        "in_transit" => "W drodze",
        "received" => "Odebrano",
        "cancelled" => "Anulowano",
        "ShipmentCreated" => "Utworzono wysyłkę",
        "ShipmentRequested" => "Złożono prośbę",
        "ShipmentApproved" => "Wysłano",
        "ShipmentInTransit" => "W drodze",
        "ShipmentReceived" => "Odebrano",
        _ => eventType
    };
}
