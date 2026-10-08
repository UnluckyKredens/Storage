using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed class GetWarehouseQueryHandler(
    IRepository<Warehouse> repository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetWarehouseQuery, WarehouseView>
{
    public async ValueTask<WarehouseView> Handle(GetWarehouseQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.WarehouseId == query.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if (!warehouseContext.IsAdministrator && !warehouseContext.CanAccess(item.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        return WarehouseMapper.ToView(item);
    }
}
