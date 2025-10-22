using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record GetProductForShipmentQuery(Guid ProductId) : IQuery<ShipmentProductView>;
