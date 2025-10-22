using MagazineAPIDomain.Enums;
using Mediator;

namespace MagazineAPIApplication.Modules.WarehouseOperations;

public sealed record CompleteWarehouseOperationCommand(
    WarehouseOperationType Type,
    Guid? WarehouseId,
    string? Notes,
    IReadOnlyList<CompleteWarehouseOperationItem> Items) : ICommand<Guid>;

public sealed record CompleteWarehouseOperationItem(
    Guid ProductId,
    Guid? SourceLocationId,
    Guid? DestinationLocationId,
    decimal Quantity,
    decimal? TargetQuantity);
