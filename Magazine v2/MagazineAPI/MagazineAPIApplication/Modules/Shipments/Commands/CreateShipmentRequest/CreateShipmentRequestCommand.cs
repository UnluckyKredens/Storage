using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record CreateShipmentRequestCommand(
    IReadOnlyList<CreateShipmentItem> Items) : ICommand<Guid>;
