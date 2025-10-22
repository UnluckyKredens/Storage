using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record ReceiveShipmentCommand(
    Guid ShipmentId,
    IReadOnlyCollection<Guid> CheckedItemIds,
    string? Notes) : ICommand<bool>;
