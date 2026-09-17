using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record StockDocumentProductOption(Guid Id, string Name, string Sku);

public sealed record StockDocumentLocationOption(
    Guid Id,
    string Code,
    Guid WarehouseId,
    string WarehouseName);

public sealed record StockDocumentWarehouseOption(Guid Id, string Name);

public sealed record StockDocumentContractorOption(
    Guid Id,
    string Name,
    ContractorType Type);

public sealed record StockDocumentPageDataView(
    IReadOnlyList<StockDocumentProductOption> Products,
    IReadOnlyList<StockDocumentLocationOption> Locations,
    IReadOnlyList<StockDocumentWarehouseOption> Warehouses,
    IReadOnlyList<StockDocumentWarehouseOption> DestinationWarehouses,
    IReadOnlyList<StockDocumentContractorOption> Contractors);
