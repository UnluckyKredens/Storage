using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record GetPurchaseOrderQuery(Guid PurchaseOrderId) : IQuery<PurchaseOrderView>;
