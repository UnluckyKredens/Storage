using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed record SaveInventoryCommand(
    Guid? Id, Guid ProductId, Guid LocationId, decimal Quantity, decimal ReservedQuantity) : ICommand<Guid>;
