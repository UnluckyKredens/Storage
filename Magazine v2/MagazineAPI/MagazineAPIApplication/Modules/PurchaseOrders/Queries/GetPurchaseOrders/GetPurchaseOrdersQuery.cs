using MagazineAPIDomain.Enums;
using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record GetPurchaseOrdersQuery(PurchaseOrderStatus? Status = null)
    : IQuery<IReadOnlyList<PurchaseOrderView>>;
