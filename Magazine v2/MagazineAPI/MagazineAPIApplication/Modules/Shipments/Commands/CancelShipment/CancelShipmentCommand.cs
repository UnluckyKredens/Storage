using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record CancelShipmentCommand(Guid ShipmentId) : ICommand<bool>;
