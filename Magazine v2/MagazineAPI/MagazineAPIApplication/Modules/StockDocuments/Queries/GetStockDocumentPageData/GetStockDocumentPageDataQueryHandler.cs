using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class GetStockDocumentPageDataQueryHandler(
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<Contractor> contractorRepository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetStockDocumentPageDataQuery, StockDocumentPageDataView>
{
    public async ValueTask<StockDocumentPageDataView> Handle(
        GetStockDocumentPageDataQuery query,
        CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var products = (await productRepository.FilterByAsync(
                product => product.IsActive,
                cancellationToken))
            .OrderBy(product => product.Name)
            .Select(product => new StockDocumentProductOption(
                product.ProductId,
                product.Name,
                product.Sku))
            .ToArray();

        var locations = warehouseContext.WarehouseId is null
            ? await locationRepository.AllAsync(cancellationToken)
            : await locationRepository.FilterByAsync(
                location => location.WarehouseId == warehouseContext.WarehouseId,
                cancellationToken);
        var allWarehouses = await warehouseRepository.AllAsync(cancellationToken);
        var warehouseIds = locations.Select(location => location.WarehouseId).ToHashSet();
        var warehouseMap = allWarehouses
            .Where(warehouse => warehouseIds.Contains(warehouse.WarehouseId))
            .ToDictionary(warehouse => warehouse.WarehouseId);

        return new StockDocumentPageDataView(
            products,
            locations
                .OrderBy(location => warehouseMap[location.WarehouseId].Name)
                .ThenBy(location => location.LocationCode)
                .Select(location => new StockDocumentLocationOption(
                    location.LocationId,
                    location.LocationCode,
                    location.WarehouseId,
                    warehouseMap[location.WarehouseId].Name))
                .ToArray(),
            warehouseMap.Values
                .OrderBy(warehouse => warehouse.Name)
                .Select(warehouse => new StockDocumentWarehouseOption(
                    warehouse.WarehouseId,
                    warehouse.Name))
                .ToArray(),
            allWarehouses
                .Where(warehouse => warehouse.WarehouseId != warehouseContext.WarehouseId)
                .OrderBy(warehouse => warehouse.Name)
                .Select(warehouse => new StockDocumentWarehouseOption(
                    warehouse.WarehouseId,
                    warehouse.Name))
                .ToArray(),
            (await contractorRepository.AllAsync(cancellationToken))
                .OrderBy(contractor => contractor.Name)
                .Select(contractor => new StockDocumentContractorOption(
                    contractor.ContractorId,
                    contractor.Name,
                    contractor.Type))
                .ToArray());
    }
}
