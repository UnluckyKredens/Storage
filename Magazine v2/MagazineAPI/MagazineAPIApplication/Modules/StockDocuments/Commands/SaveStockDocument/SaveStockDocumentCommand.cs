using MagazineAPIDomain.Enums;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record SaveStockDocumentItem(
    Guid ProductId,
    Guid LocationId,
    decimal Quantity);

public sealed record SaveStockDocumentCommand(
    Guid? Id,
    StockDocumentType Type,
    Guid WarehouseId,
    Guid? ContractorId,
    Guid? DestinationWarehouseId,
    string? Notes,
    IReadOnlyList<SaveStockDocumentItem> Items) : ICommand<Guid>;
