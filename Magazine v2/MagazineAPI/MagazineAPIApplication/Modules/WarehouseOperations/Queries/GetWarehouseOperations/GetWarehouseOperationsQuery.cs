using Mediator;

namespace MagazineAPIApplication.Modules.WarehouseOperations;

public sealed record GetWarehouseOperationsQuery : IQuery<IReadOnlyList<WarehouseOperationView>>;
