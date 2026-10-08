using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

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
        var products = await productRepository.Query()
            .Where(product => product.IsActive)
            .OrderBy(product => product.Name)
            .Select(product => new ProductOption(product.ProductId, product.Name, product.Sku))
            .ToArrayAsync(cancellationToken);
        var locationQuery = locationRepository.Query();
        if (warehouseContext.WarehouseId is { } activeWarehouseId)
        {
            locationQuery = locationQuery.Where(location => location.WarehouseId == activeWarehouseId);
        }

        var locationItems = await locationQuery.ToArrayAsync(cancellationToken);
        var warehouseIds = locationItems.Select(location => location.WarehouseId).ToHashSet();
        var warehouses = await warehouseRepository.Query()
            .Where(warehouse => warehouseIds.Contains(warehouse.WarehouseId))
            .ToDictionaryAsync(x => x.WarehouseId, cancellationToken);
        var locations = locationItems
            .Select(x => new LocationOption(x.LocationId, x.LocationCode, x.WarehouseId, warehouses[x.WarehouseId].Name))
            .OrderBy(x => x.WarehouseName).ThenBy(x => x.Code).ToArray();
        return new InventoryPageDataView(products, locations);
    }
}
