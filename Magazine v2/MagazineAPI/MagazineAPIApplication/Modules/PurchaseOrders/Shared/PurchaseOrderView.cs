namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record PurchaseOrderView(
    Guid Id,
    string Number,
    Guid WarehouseId,
    string WarehouseName,
    Guid ContractorId,
    string ContractorName,
    string Status,
    DateTime CreatedOnUtc,
    DateTime? ApprovedOnUtc,
    DateTime? ReceivedOnUtc,
    string? InvoiceNumber,
    string? PaperDocumentNumber,
    string? Notes,
    decimal TotalValue,
    IReadOnlyList<PurchaseOrderItemView> Items);
