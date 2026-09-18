using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class GetProductForShipmentQueryHandler(
    IRepository<Product> productRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetProductForShipmentQuery, ShipmentProductView>
{
    public async ValueTask<ShipmentProductView> Handle(
        GetProductForShipmentQuery query,
        CancellationToken cancellationToken)
    {
        var sourceWarehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn źródłowy.");

        var product = await productRepository.FirstOrDefaultAsync(
            candidate => candidate.ProductId == query.ProductId && candidate.IsActive,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Nie znaleziono aktywnego produktu.");

        var sourceLocationIds = (await locationRepository.FilterByAsync(
                location => location.WarehouseId == sourceWarehouseId,
                cancellationToken))
            .Select(location => location.LocationId)
            .ToHashSet();

        var availableQuantity = (await inventoryRepository.FilterByAsync(
                inventory => inventory.ProductId == product.ProductId,
                cancellationToken))
            .Where(inventory => sourceLocationIds.Contains(inventory.LocationId))
            .Sum(inventory => inventory.AvailableQuantity);

        return new ShipmentProductView(
            product.ProductId,
            product.Name,
            product.Sku,
            product.Barcode ?? product.Sku,
            availableQuantity);
    }
}
