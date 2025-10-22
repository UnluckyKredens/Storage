namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record PurchaseOrderItemView(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    string Barcode,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
