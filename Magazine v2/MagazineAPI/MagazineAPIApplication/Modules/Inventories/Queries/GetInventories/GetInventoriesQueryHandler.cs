using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed class GetInventoriesQueryHandler(
    IRepository<Inventory> inventoryRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetInventoriesQuery, IReadOnlyList<InventoryView>>
{
    public async ValueTask<IReadOnlyList<InventoryView>> Handle(GetInventoriesQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var locationsQuery = locationRepository.Query();
        if (warehouseContext.WarehouseId is { } activeWarehouseId)
        {
            locationsQuery = locationsQuery.Where(location => location.WarehouseId == activeWarehouseId);
        }

        var locationsList = await locationsQuery.ToListAsync(cancellationToken);
        var locationIds = locationsList.Select(location => location.LocationId).ToHashSet();
        var inventories = await inventoryRepository.Query()
            .Where(inventory => locationIds.Contains(inventory.LocationId))
            .ToListAsync(cancellationToken);
        var productIds = inventories.Select(inventory => inventory.ProductId).ToHashSet();
        var products = await productRepository.Query()
            .Where(product => productIds.Contains(product.ProductId))
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        var locations = locationsList.ToDictionary(x => x.LocationId);
        var warehouseIds = locationsList.Select(location => location.WarehouseId).ToHashSet();
        var warehouses = await warehouseRepository.Query()
            .Where(warehouse => warehouseIds.Contains(warehouse.WarehouseId))
            .ToDictionaryAsync(x => x.WarehouseId, cancellationToken);
        return inventories.Select(item =>
        {
            var location = locations[item.LocationId];
            return new InventoryView(item.InventoryId, item.ProductId, products[item.ProductId].Name,
                item.LocationId, location.LocationCode, location.WarehouseId,
                warehouses[location.WarehouseId].Name, item.Quantity, item.ReservedQuantity, item.AvailableQuantity);
        }).ToArray();
    }
}
