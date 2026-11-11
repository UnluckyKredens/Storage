using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record GetPurchaseOrdersQuery : IQuery<IReadOnlyList<PurchaseOrderView>>;
