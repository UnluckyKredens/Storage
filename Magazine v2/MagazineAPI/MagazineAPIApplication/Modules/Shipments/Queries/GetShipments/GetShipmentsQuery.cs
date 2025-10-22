using Mediator;
using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record GetShipmentsQuery(bool ReadyOnly = false, ShipmentStatus? Status = null)
    : IQuery<IReadOnlyList<ShipmentView>>;
