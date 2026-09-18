using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed class GetLocationQueryHandler(
    IRepository<Location> repository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetLocationQuery, LocationView>
{
    public async ValueTask<LocationView> Handle(GetLocationQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.LocationId == query.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if (!warehouseContext.CanAccess(item.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        var warehouse = await warehouseRepository.FirstOrDefaultAsync(x => x.WarehouseId == item.WarehouseId, cancellationToken);
        return LocationMapper.ToView(item, warehouse?.Name ?? string.Empty);
    }
}
