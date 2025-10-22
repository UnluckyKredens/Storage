using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record GetShipmentPageDataQuery() : IQuery<ShipmentPageDataView>;
