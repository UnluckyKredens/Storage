using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

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
        var locationsList = warehouseContext.WarehouseId is null
            ? await locationRepository.AllAsync(cancellationToken)
            : await locationRepository.FilterByAsync(
                location => location.WarehouseId == warehouseContext.WarehouseId,
                cancellationToken);
        var locationIds = locationsList.Select(location => location.LocationId).ToHashSet();
        var inventories = (await inventoryRepository.AllAsync(cancellationToken))
            .Where(inventory => locationIds.Contains(inventory.LocationId));
        var products = (await productRepository.AllAsync(cancellationToken)).ToDictionary(x => x.ProductId);
        var locations = locationsList.ToDictionary(x => x.LocationId);
        var warehouses = (await warehouseRepository.AllAsync(cancellationToken)).ToDictionary(x => x.WarehouseId);
        return inventories.Select(item =>
        {
            var location = locations[item.LocationId];
            return new InventoryView(item.InventoryId, item.ProductId, products[item.ProductId].Name,
                item.LocationId, location.LocationCode, location.WarehouseId,
                warehouses[location.WarehouseId].Name, item.Quantity, item.ReservedQuantity, item.AvailableQuantity);
        }).ToArray();
    }
}
