using MagazineAPIApplication.Modules.Warehouses;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed record PurchaseOrderPageDataView(
    WarehouseView Warehouse,
    IReadOnlyList<PurchaseOrderContractorView> Suppliers,
    IReadOnlyList<PurchaseOrderProductView> Products);

public sealed record PurchaseOrderContractorView(Guid Id, string Name, string TaxNumber);

public sealed record PurchaseOrderProductView(
    Guid ProductId,
    string Name,
    string Sku,
    string Barcode,
    decimal PurchasePrice);
