using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record GetShipmentByIdentifierQuery(string Identifier) : IQuery<ShipmentView>;
