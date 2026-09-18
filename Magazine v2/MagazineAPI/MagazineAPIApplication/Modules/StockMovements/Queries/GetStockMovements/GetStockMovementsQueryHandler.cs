using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.StockMovements;

public sealed class GetStockMovementsQueryHandler(
    IRepository<StockMovement> movementRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<User> userRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementView>>
{
    public async ValueTask<IReadOnlyList<StockMovementView>> Handle(
        GetStockMovementsQuery query,
        CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var limit = Math.Clamp(query.Limit, 1, 1000);
        var movementsQuery = movementRepository.Query();

        if (query.ProductId is not null)
            movementsQuery = movementsQuery.Where(movement => movement.ProductId == query.ProductId);
        if (query.WarehouseId is not null)
            movementsQuery = movementsQuery.Where(movement => movement.WarehouseId == query.WarehouseId);
        if (query.SourceId is not null)
            movementsQuery = movementsQuery.Where(movement => movement.SourceId == query.SourceId);
        if (warehouseContext.WarehouseId is not null)
            movementsQuery = movementsQuery.Where(movement => movement.WarehouseId == warehouseContext.WarehouseId);

        var orderedMovements = await movementsQuery
            .OrderByDescending(movement => movement.CreatedOnUtc)
            .Take(limit)
            .ToArrayAsync(cancellationToken);

        var productIds = orderedMovements.Select(movement => movement.ProductId).ToHashSet();
        var locationIds = orderedMovements.Select(movement => movement.LocationId).ToHashSet();
        var warehouseIds = orderedMovements.Select(movement => movement.WarehouseId).ToHashSet();
        var userIds = orderedMovements.Select(movement => movement.CreatedByUserId).ToHashSet();
        var products = await productRepository.Query()
            .Where(item => productIds.Contains(item.ProductId))
            .ToDictionaryAsync(item => item.ProductId, cancellationToken);
        var locations = await locationRepository.Query()
            .Where(item => locationIds.Contains(item.LocationId))
            .ToDictionaryAsync(item => item.LocationId, cancellationToken);
        var warehouses = await warehouseRepository.Query()
            .Where(item => warehouseIds.Contains(item.WarehouseId))
            .ToDictionaryAsync(item => item.WarehouseId, cancellationToken);
        var users = await userRepository.Query()
            .Where(item => userIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);

        return orderedMovements
            .Select(movement =>
            {
                var product = products.GetValueOrDefault(movement.ProductId);
                var location = locations.GetValueOrDefault(movement.LocationId);
                var warehouse = warehouses.GetValueOrDefault(movement.WarehouseId);
                var user = users.GetValueOrDefault(movement.CreatedByUserId);

                return new StockMovementView(
                    movement.StockMovementId,
                    movement.CreatedOnUtc,
                    movement.Type,
                    movement.WarehouseId,
                    warehouse?.Name ?? string.Empty,
                    movement.LocationId,
                    location?.LocationCode ?? string.Empty,
                    movement.ProductId,
                    product?.Name ?? string.Empty,
                    movement.InventoryId,
                    movement.QuantityBefore,
                    movement.QuantityChange,
                    movement.QuantityAfter,
                    movement.ReservedQuantityBefore,
                    movement.ReservedQuantityChange,
                    movement.ReservedQuantityAfter,
                    movement.SourceType,
                    movement.SourceId,
                    movement.SourceNumber,
                    movement.CreatedByUserId,
                    user is null ? string.Empty : $"{user.FirstName} {user.LastName}",
                    movement.Notes);
            })
            .ToArray();
    }
}
