using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record PackShipmentCommand(Guid ShipmentId) : ICommand<bool>;
