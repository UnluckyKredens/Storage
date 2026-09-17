using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed class GetWarehousesQueryHandler(
    IRepository<Warehouse> repository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetWarehousesQuery, IReadOnlyList<WarehouseView>>
{
    public async ValueTask<IReadOnlyList<WarehouseView>> Handle(GetWarehousesQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var items = warehouseContext.IsAdministrator
            ? await repository.AllAsync(cancellationToken)
            : await repository.FilterByAsync(
                warehouse => warehouse.WarehouseId == warehouseContext.WarehouseId,
                cancellationToken);
        return items.Select(WarehouseMapper.ToView).ToArray();
    }
}
