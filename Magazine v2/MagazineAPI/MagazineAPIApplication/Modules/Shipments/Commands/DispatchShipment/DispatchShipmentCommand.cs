using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record DispatchShipmentCommand(Guid ShipmentId) : ICommand<bool>;
