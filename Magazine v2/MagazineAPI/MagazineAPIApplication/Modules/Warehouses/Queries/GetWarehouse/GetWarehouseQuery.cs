using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed record GetWarehouseQuery(Guid Id) : IQuery<WarehouseView>;
