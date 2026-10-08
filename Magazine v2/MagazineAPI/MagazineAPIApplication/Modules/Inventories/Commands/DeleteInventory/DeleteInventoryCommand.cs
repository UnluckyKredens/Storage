using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed record DeleteInventoryCommand(Guid Id) : ICommand<bool>;
