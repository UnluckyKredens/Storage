using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record ApprovePurchaseOrderCommand(
    Guid PurchaseOrderId,
    string InvoiceNumber,
    string PaperDocumentNumber,
    string? Notes) : ICommand<bool>;
