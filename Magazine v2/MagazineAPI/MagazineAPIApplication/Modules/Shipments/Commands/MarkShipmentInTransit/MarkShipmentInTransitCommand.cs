using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record MarkShipmentInTransitCommand(Guid ShipmentId) : ICommand<bool>;
