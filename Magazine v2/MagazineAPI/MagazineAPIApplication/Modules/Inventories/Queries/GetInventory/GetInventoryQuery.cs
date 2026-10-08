using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed record GetInventoryQuery(Guid Id) : IQuery<InventoryView>;
