using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed record GetInventoriesQuery() : IQuery<IReadOnlyList<InventoryView>>;
