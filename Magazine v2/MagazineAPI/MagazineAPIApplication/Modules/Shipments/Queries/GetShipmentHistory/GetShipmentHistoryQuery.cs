using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record GetShipmentHistoryQuery(Guid ShipmentId) : IQuery<IReadOnlyList<ShipmentHistoryView>>;
