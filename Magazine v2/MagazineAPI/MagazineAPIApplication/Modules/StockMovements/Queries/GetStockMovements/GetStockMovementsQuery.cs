using Mediator;

namespace MagazineAPIApplication.Modules.StockMovements;

public sealed record GetStockMovementsQuery(
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    Guid? SourceId = null,
    int Limit = 500) : IQuery<IReadOnlyList<StockMovementView>>;
