namespace MagazineAPIApplication.Modules.Shipments;

public sealed record ShipmentView(
    Guid Id,
    string Number,
    Guid SourceWarehouseId,
    string SourceWarehouseName,
    Guid DestinationWarehouseId,
    string DestinationWarehouseName,
    string Status,
    DateTime CreatedOnUtc,
    DateTime? ApprovedOnUtc,
    DateTime? ReceivedOnUtc,
    IReadOnlyList<ShipmentItemView> Items);
