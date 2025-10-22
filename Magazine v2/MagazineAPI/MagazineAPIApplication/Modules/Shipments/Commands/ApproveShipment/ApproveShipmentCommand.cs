using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record ApproveShipmentCommand(Guid ShipmentId) : ICommand<bool>;
