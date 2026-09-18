using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed class GetInventoryQueryHandler(
    IRepository<Inventory> inventoryRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetInventoryQuery, InventoryView>
{
    public async ValueTask<InventoryView> Handle(GetInventoryQuery query, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.FirstOrDefaultAsync(x => x.InventoryId == query.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Stan magazynowy nie został znaleziony.");
        var product = await productRepository.FirstOrDefaultAsync(x => x.ProductId == item.ProductId, cancellationToken);
        var location = await locationRepository.FirstOrDefaultAsync(x => x.LocationId == item.LocationId, cancellationToken)
            ?? throw new ResourceNotFoundException("Lokalizacja nie została znaleziona.");
        if (!warehouseContext.CanAccess(location.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        var warehouse = location is null ? null : await warehouseRepository.FirstOrDefaultAsync(x => x.WarehouseId == location.WarehouseId, cancellationToken);
        return new InventoryView(item.InventoryId, item.ProductId, product?.Name ?? string.Empty,
            item.LocationId, location?.LocationCode ?? string.Empty, location?.WarehouseId ?? Guid.Empty,
            warehouse?.Name ?? string.Empty, item.Quantity, item.ReservedQuantity, item.AvailableQuantity);
    }
}
