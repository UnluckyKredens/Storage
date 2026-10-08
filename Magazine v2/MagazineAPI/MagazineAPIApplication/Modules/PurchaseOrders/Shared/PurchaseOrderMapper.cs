using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

internal static class PurchaseOrderMapper
{
    public static PurchaseOrderView ToView(
        PurchaseOrder order,
        IReadOnlyDictionary<Guid, Warehouse> warehouses,
        IReadOnlyDictionary<Guid, Contractor> contractors,
        IReadOnlyDictionary<Guid, Product> products)
    {
        var warehouse = warehouses.GetValueOrDefault(order.WarehouseId);
        var contractor = contractors.GetValueOrDefault(order.ContractorId);
        var items = order.Items
            .OrderBy(item => products.GetValueOrDefault(item.ProductId)?.Name ?? string.Empty)
            .Select(item =>
            {
                var product = products.GetValueOrDefault(item.ProductId);
                return new PurchaseOrderItemView(
                    item.PurchaseOrderItemId,
                    item.ProductId,
                    product?.Name ?? string.Empty,
                    product?.Sku ?? string.Empty,
                    item.Barcode,
                    item.Quantity,
                    item.UnitPrice,
                    item.Quantity * item.UnitPrice);
            })
            .ToArray();

        return new PurchaseOrderView(
            order.PurchaseOrderId,
            order.Number,
            order.WarehouseId,
            warehouse?.Name ?? string.Empty,
            order.ContractorId,
            contractor?.Name ?? string.Empty,
            DisplayStatus(order.Status),
            order.CreatedOnUtc,
            order.ApprovedOnUtc,
            order.ReceivedOnUtc,
            order.InvoiceNumber,
            order.PaperDocumentNumber,
            order.Notes,
            items.Sum(item => item.TotalPrice),
            items);
    }

    private static string DisplayStatus(PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.PendingApproval => "Oczekuje na akceptację",
        PurchaseOrderStatus.Approved => "Zaakceptowane",
        PurchaseOrderStatus.Received => "Przyjęte",
        _ => status.ToString()
    };
}
