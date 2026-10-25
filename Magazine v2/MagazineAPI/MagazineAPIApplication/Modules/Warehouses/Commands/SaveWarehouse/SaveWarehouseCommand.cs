using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed record SaveWarehouseCommand(
    Guid? Id,
    string Name,
    string? Address,
    string? Description,
    bool? IsHub = null) : ICommand<WarehouseView>;
