using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed record SaveLocationCommand(
    Guid? Id,
    Guid WarehouseId,
    string LocationCode,
    string? Description) : ICommand<LocationView>;
