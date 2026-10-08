using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class GetProductByBarcodeQueryHandler(
    IRepository<Product> productRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetProductByBarcodeQuery, ShipmentProductView>
{
    public async ValueTask<ShipmentProductView> Handle(GetProductByBarcodeQuery query, CancellationToken cancellationToken)
    {
        var sourceWarehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn źródłowy.");
        var barcode = query.Barcode.Trim();
        if (string.IsNullOrWhiteSpace(barcode))
            throw new CommandValidationException("Kod kreskowy jest wymagany.");

        var product = await productRepository.FirstOrDefaultAsync(
            candidate => candidate.Barcode == barcode && candidate.IsActive,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Nie znaleziono aktywnego produktu z podanym kodem.");

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
            barcode,
            availableQuantity);
    }
}
