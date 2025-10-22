using MagazineAPIApplication.Modules.Warehouses;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed record ShipmentPageDataView(
    WarehouseView SourceWarehouse,
    IReadOnlyList<WarehouseView> DestinationWarehouses,
    IReadOnlyList<ShipmentProductView> AvailableProducts);
