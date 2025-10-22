namespace MagazineAPIApplication.Modules.Shipments;

public sealed record ShipmentItemView(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    string Barcode,
    decimal Quantity);
