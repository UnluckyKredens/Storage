using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record GetShipmentQuery(Guid ShipmentId) : IQuery<ShipmentView>;
