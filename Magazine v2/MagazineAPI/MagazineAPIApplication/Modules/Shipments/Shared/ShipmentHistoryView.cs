namespace MagazineAPIApplication.Modules.Shipments;

public sealed record ShipmentHistoryView(
    Guid Id,
    string EventType,
    string EventName,
    Guid SourceWarehouseId,
    string SourceWarehouseName,
    Guid DestinationWarehouseId,
    string DestinationWarehouseName,
    Guid? UserId,
    string? UserName,
    DateTime CreatedOnUtc,
    string Details);
