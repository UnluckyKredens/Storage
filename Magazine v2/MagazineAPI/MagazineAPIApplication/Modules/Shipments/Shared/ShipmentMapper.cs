using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.Shipments;

internal static class ShipmentMapper
{
    public static ShipmentView ToView(
        Shipment shipment,
        IReadOnlyDictionary<Guid, Warehouse> warehouses,
        IReadOnlyDictionary<Guid, Product> products)
    {
        var source = warehouses.GetValueOrDefault(shipment.SourceWarehouseId);
        var destination = warehouses.GetValueOrDefault(shipment.DestinationWarehouseId);

        return new ShipmentView(
            shipment.ShipmentId,
            shipment.Number,
            shipment.SourceWarehouseId,
            source?.Name ?? string.Empty,
            shipment.DestinationWarehouseId,
            destination?.Name ?? string.Empty,
            DisplayStatus(shipment.Status),
            shipment.CreatedOnUtc,
            shipment.ApprovedOnUtc,
            shipment.ReceivedOnUtc,
            shipment.Items
                .OrderBy(item => products.GetValueOrDefault(item.ProductId)?.Name ?? string.Empty)
                .Select(item =>
                {
                    var product = products.GetValueOrDefault(item.ProductId);
                    return new ShipmentItemView(
                        item.ShipmentItemId,
                        item.ProductId,
                        product?.Name ?? string.Empty,
                        product?.Sku ?? string.Empty,
                        item.Barcode,
                        item.Quantity);
                })
                .ToArray());
    }

    private static string DisplayStatus(ShipmentStatus status) => status switch
    {
        ShipmentStatus.PendingApproval => "Oczekuje na akceptację",
        ShipmentStatus.Sent => "Wysłana",
        ShipmentStatus.InTransit => "W drodze",
        ShipmentStatus.Received => "Odebrana",
        _ => status.ToString()
    };
}
