using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed record DeleteWarehouseCommand(Guid Id) : ICommand<bool>;
