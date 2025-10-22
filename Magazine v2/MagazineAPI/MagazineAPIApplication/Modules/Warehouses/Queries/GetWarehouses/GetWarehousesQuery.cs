using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed record GetWarehousesQuery() : IQuery<IReadOnlyList<WarehouseView>>;
