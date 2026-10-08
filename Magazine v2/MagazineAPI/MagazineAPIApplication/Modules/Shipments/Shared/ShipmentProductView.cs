namespace MagazineAPIApplication.Modules.Shipments;

public sealed record ShipmentProductView(
    Guid ProductId,
    string Name,
    string Sku,
    string Barcode,
    decimal AvailableQuantity);
