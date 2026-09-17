using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed class GetLocationsQueryHandler(
    IRepository<Location> repository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetLocationsQuery, IReadOnlyList<LocationView>>
{
    public async ValueTask<IReadOnlyList<LocationView>> Handle(GetLocationsQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var items = warehouseContext.WarehouseId is null
            ? await repository.AllAsync(cancellationToken)
            : await repository.FilterByAsync(
                location => location.WarehouseId == warehouseContext.WarehouseId,
                cancellationToken);
        var warehouses = (await warehouseRepository.AllAsync(cancellationToken)).ToDictionary(x => x.WarehouseId);
        return items.Select(x => LocationMapper.ToView(x, warehouses.GetValueOrDefault(x.WarehouseId)?.Name ?? string.Empty)).ToArray();
    }
}
