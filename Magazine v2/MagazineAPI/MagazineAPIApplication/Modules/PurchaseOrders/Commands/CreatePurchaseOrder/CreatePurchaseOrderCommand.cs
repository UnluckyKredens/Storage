using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record CreatePurchaseOrderCommand(
    Guid ContractorId,
    IReadOnlyList<CreatePurchaseOrderItem> Items,
    string? Notes) : ICommand<Guid>;

public sealed record CreatePurchaseOrderItem(Guid ProductId, decimal Quantity, decimal? UnitPrice);
