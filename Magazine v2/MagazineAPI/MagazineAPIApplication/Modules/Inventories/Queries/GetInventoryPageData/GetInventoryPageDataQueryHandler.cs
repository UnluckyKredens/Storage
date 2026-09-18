using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed class GetInventoryPageDataQueryHandler(
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetInventoryPageDataQuery, InventoryPageDataView>
{
    public async ValueTask<InventoryPageDataView> Handle(GetInventoryPageDataQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var products = (await productRepository.FilterByAsync(x => x.IsActive, cancellationToken))
            .Select(x => new ProductOption(x.ProductId, x.Name, x.Sku)).OrderBy(x => x.Name).ToArray();
        var warehouses = (await warehouseRepository.AllAsync(cancellationToken)).ToDictionary(x => x.WarehouseId);
        var locationItems = warehouseContext.WarehouseId is null
            ? await locationRepository.AllAsync(cancellationToken)
            : await locationRepository.FilterByAsync(
                location => location.WarehouseId == warehouseContext.WarehouseId,
                cancellationToken);
        var locations = locationItems
            .Select(x => new LocationOption(x.LocationId, x.LocationCode, x.WarehouseId, warehouses[x.WarehouseId].Name))
            .OrderBy(x => x.WarehouseName).ThenBy(x => x.Code).ToArray();
        return new InventoryPageDataView(products, locations);
    }
}
