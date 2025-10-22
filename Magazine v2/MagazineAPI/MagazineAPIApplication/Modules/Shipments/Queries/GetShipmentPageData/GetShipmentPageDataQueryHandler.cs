using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.Warehouses;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class GetShipmentPageDataQueryHandler(
    IRepository<Warehouse> warehouseRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Inventory> inventoryRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetShipmentPageDataQuery, ShipmentPageDataView>
{
    public async ValueTask<ShipmentPageDataView> Handle(GetShipmentPageDataQuery query, CancellationToken cancellationToken)
    {
        var sourceWarehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn źródłowy.");
        var sourceWarehouse = await warehouseRepository.FirstOrDefaultAsync(
            warehouse => warehouse.Id == sourceWarehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn źródłowy nie istnieje.");

        var destinationWarehouses = await warehouseRepository.Query()
            .Where(warehouse => warehouse.Id != sourceWarehouseId)
            .OrderBy(warehouse => warehouse.Name)
            .ToArrayAsync(cancellationToken);
        var destinations = destinationWarehouses
            .Select(WarehouseMapper.ToView)
            .ToArray();

        var sourceLocationIds = (await locationRepository.Query()
                .Where(location => location.WarehouseId == sourceWarehouseId)
                .ToListAsync(cancellationToken))
            .Select(location => location.Id)
            .ToHashSet();

        var availableByProduct = await inventoryRepository.Query()
            .Where(inventory => sourceLocationIds.Contains(inventory.LocationId))
            .GroupBy(inventory => inventory.ProductId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Sum(inventory => inventory.AvailableQuantity),
                cancellationToken);
        var availableProductIds = availableByProduct
            .Where(pair => pair.Value > 0)
            .Select(pair => pair.Key)
            .ToHashSet();

        var productRows = await productRepository.Query()
            .Where(product => product.IsActive && availableProductIds.Contains(product.Id))
            .OrderBy(product => product.Name)
            .ToArrayAsync(cancellationToken);
        var availableProducts = productRows
            .Select(product => new ShipmentProductView(
                product.Id,
                product.Name,
                product.Sku,
                product.Barcode ?? product.Sku,
                availableByProduct.GetValueOrDefault(product.Id)))
            .ToArray();

        return new ShipmentPageDataView(
            WarehouseMapper.ToView(sourceWarehouse),
            destinations,
            availableProducts);
    }
}
