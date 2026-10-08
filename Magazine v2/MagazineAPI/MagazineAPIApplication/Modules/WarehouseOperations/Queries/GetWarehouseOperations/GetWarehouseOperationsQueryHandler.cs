using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.WarehouseOperations;

public sealed class GetWarehouseOperationsQueryHandler(
    IRepository<WarehouseOperation> operationRepository,
    IRepository<WarehouseOperationItem> operationItemRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetWarehouseOperationsQuery, IReadOnlyList<WarehouseOperationView>>
{
    public async ValueTask<IReadOnlyList<WarehouseOperationView>> Handle(GetWarehouseOperationsQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var operationsQuery = operationRepository.Query();
        if (warehouseContext.WarehouseId is { } warehouseId)
        {
            operationsQuery = operationsQuery.Where(operation => operation.WarehouseId == warehouseId);
        }

        var operations = await operationsQuery
            .OrderByDescending(operation => operation.CompletedOnUtc)
            .Take(500)
            .ToArrayAsync(cancellationToken);
        var operationIds = operations.Select(operation => operation.WarehouseOperationId).ToHashSet();
        var items = (await operationItemRepository.Query()
            .Where(item => operationIds.Contains(item.WarehouseOperationId))
            .ToListAsync(cancellationToken))
            .ToLookup(item => item.WarehouseOperationId);
        var flatItems = items.SelectMany(group => group).ToArray();
        var productIds = flatItems.Select(item => item.ProductId).ToHashSet();
        var locationIds = items
            .SelectMany(group => group)
            .SelectMany(item => new[] { item.SourceLocationId, item.DestinationLocationId })
            .OfType<Guid>()
            .ToHashSet();
        var warehouseIds = operations.Select(operation => operation.WarehouseId).ToHashSet();
        var products = await productRepository.Query()
            .Where(product => productIds.Contains(product.ProductId))
            .ToDictionaryAsync(product => product.ProductId, cancellationToken);
        var locations = await locationRepository.Query()
            .Where(location => locationIds.Contains(location.LocationId))
            .ToDictionaryAsync(location => location.LocationId, cancellationToken);
        var warehouses = await warehouseRepository.Query()
            .Where(warehouse => warehouseIds.Contains(warehouse.WarehouseId))
            .ToDictionaryAsync(warehouse => warehouse.WarehouseId, cancellationToken);

        return operations
            .Select(operation =>
            {
                var warehouse = warehouses.GetValueOrDefault(operation.WarehouseId);
                return new WarehouseOperationView(
                    operation.WarehouseOperationId,
                    operation.Number,
                    operation.Type.ToString(),
                    operation.Status.ToString(),
                    operation.WarehouseId,
                    warehouse?.Name ?? string.Empty,
                    operation.CompletedOnUtc,
                    operation.Notes,
                    items[operation.WarehouseOperationId]
                        .Select(item =>
                        {
                            var product = products.GetValueOrDefault(item.ProductId);
                            var source = item.SourceLocationId is { } sourceId
                                ? locations.GetValueOrDefault(sourceId)
                                : null;
                            var destination = item.DestinationLocationId is { } destinationId
                                ? locations.GetValueOrDefault(destinationId)
                                : null;
                            return new WarehouseOperationItemView(
                                item.WarehouseOperationItemId,
                                item.ProductId,
                                product?.Name ?? string.Empty,
                                source?.LocationCode,
                                destination?.LocationCode,
                                item.Quantity,
                                item.TargetQuantity);
                        })
                        .ToArray());
            })
            .ToArray();
    }
}
