using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record ReceivePurchaseOrderCommand(
    Guid PurchaseOrderId,
    IReadOnlyCollection<Guid> CheckedItemIds,
    string? Notes) : ICommand<bool>;
