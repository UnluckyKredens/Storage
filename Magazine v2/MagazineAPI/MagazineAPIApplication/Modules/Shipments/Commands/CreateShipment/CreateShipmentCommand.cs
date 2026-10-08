using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record CreateShipmentCommand(
    Guid DestinationWarehouseId,
    IReadOnlyList<CreateShipmentItem> Items) : ICommand<Guid>;

public sealed record CreateShipmentItem(string? Barcode, Guid? ProductId, decimal Quantity);
