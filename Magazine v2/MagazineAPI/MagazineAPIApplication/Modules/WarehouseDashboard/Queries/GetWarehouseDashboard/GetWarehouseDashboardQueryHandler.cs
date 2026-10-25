using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.WarehouseDashboard;

public sealed class GetWarehouseDashboardQueryHandler(
    IRepository<Inventory> inventoryRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<StockReservation> reservationRepository,
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetWarehouseDashboardQuery, WarehouseDashboardView>
{
    public async ValueTask<WarehouseDashboardView> Handle(GetWarehouseDashboardQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var locationsQuery = locationRepository.Query();
        if (warehouseContext.WarehouseId is { } activeWarehouseId)
        {
            locationsQuery = locationsQuery.Where(location => location.WarehouseId == activeWarehouseId);
        }

        var locations = await locationsQuery.ToListAsync(cancellationToken);
        var locationIds = locations.Select(location => location.Id).ToHashSet();
        var warehouseIds = locations.Select(location => location.WarehouseId).ToHashSet();
        var warehouses = await warehouseRepository.Query()
            .Where(warehouse => warehouseIds.Contains(warehouse.Id))
            .ToDictionaryAsync(warehouse => warehouse.Id, cancellationToken);

        var inventories = await inventoryRepository.Query()
            .Where(inventory => locationIds.Contains(inventory.LocationId))
            .ToArrayAsync(cancellationToken);
        var productIds = inventories.Select(inventory => inventory.ProductId).ToHashSet();
        var products = await productRepository.Query()
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);
        var activeReservationCount = await reservationRepository.Query()
            .Where(reservation =>
                warehouseIds.Contains(reservation.WarehouseId) &&
                reservation.Status == StockReservationStatus.Active)
            .CountAsync(cancellationToken);
        var shipmentCounts = await shipmentRepository.Query()
            .Where(shipment =>
                warehouseIds.Contains(shipment.SourceWarehouseId) ||
                warehouseIds.Contains(shipment.DestinationWarehouseId))
            .GroupBy(shipment => shipment.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);

        var inventoryByProductAndWarehouse = inventories
            .Select(inventory =>
            {
                var location = locations.First(location => location.Id == inventory.LocationId);
                return new { Inventory = inventory, location.WarehouseId };
            })
            .GroupBy(item => new { item.WarehouseId, item.Inventory.ProductId });

        var alerts = inventoryByProductAndWarehouse
            .Select(group =>
            {
                var product = products.GetValueOrDefault(group.Key.ProductId);
                var warehouse = warehouses.GetValueOrDefault(group.Key.WarehouseId);
                var quantity = group.Sum(item => item.Inventory.Quantity);
                var reservedQuantity = group.Sum(item => item.Inventory.ReservedQuantity);
                var availableQuantity = group.Sum(item => item.Inventory.AvailableQuantity);
                var minimumQuantity = product?.MinimumQuantity ?? 0;
                var optimumQuantity = product?.OptimumQuantity;

                if (reservedQuantity > 0 && availableQuantity == 0 && quantity > 0)
                {
                    return new WarehouseDashboardAlertView(
                        "FullyReserved",
                        group.Key.WarehouseId,
                        warehouse?.Name ?? string.Empty,
                        group.Key.ProductId,
                        product?.Name ?? string.Empty,
                        availableQuantity,
                        minimumQuantity,
                        optimumQuantity,
                        "Cały stan jest zarezerwowany.");
                }


                if (availableQuantity <= 0)
                {
                    return new WarehouseDashboardAlertView(
                        "OutOfStock",
                        group.Key.WarehouseId,
                        warehouse?.Name ?? string.Empty,
                        group.Key.ProductId,
                        product?.Name ?? string.Empty,
                        availableQuantity,
                        minimumQuantity,
                        optimumQuantity,
                        "Brak dostępnego stanu.");
                }

                if (minimumQuantity > 0 && availableQuantity <= minimumQuantity)
                {
                    return new WarehouseDashboardAlertView(
                        "BelowMinimum",
                        group.Key.WarehouseId,
                        warehouse?.Name ?? string.Empty,
                        group.Key.ProductId,
                        product?.Name ?? string.Empty,
                        availableQuantity,
                        minimumQuantity,
                        optimumQuantity,
                        "Stan dostępny jest poniżej minimum.");
                }

                if (optimumQuantity is not null && availableQuantity < optimumQuantity)
                {
                    return new WarehouseDashboardAlertView(
                        "BelowOptimum",
                        group.Key.WarehouseId,
                        warehouse?.Name ?? string.Empty,
                        group.Key.ProductId,
                        product?.Name ?? string.Empty,
                        availableQuantity,
                        minimumQuantity,
                        optimumQuantity,
                        "Stan dostępny jest poniżej optimum.");
                }


                return null;
            })
            .Where(alert => alert is not null)
            .Cast<WarehouseDashboardAlertView>()
            .OrderBy(alert => alert.WarehouseName)
            .ThenBy(alert => alert.ProductName)
            .Take(50)
            .ToArray();

        var backorders = await BuildBackordersAsync(warehouseIds, cancellationToken);

        return new WarehouseDashboardView(
            inventories.Select(inventory => inventory.ProductId).Distinct().Count(),
            inventories.Length,
            inventories.Sum(inventory => inventory.Quantity),
            inventories.Sum(inventory => inventory.ReservedQuantity),
            inventories.Sum(inventory => inventory.AvailableQuantity),
            activeReservationCount,
            shipmentCounts.GetValueOrDefault(ShipmentStatus.PendingApproval),
            shipmentCounts.GetValueOrDefault(ShipmentStatus.Sent),
            shipmentCounts.GetValueOrDefault(ShipmentStatus.InTransit),
            alerts,
            backorders);
    }

    private async Task<IReadOnlyList<WarehouseDashboardBackorderView>> BuildBackordersAsync(
        IReadOnlySet<Guid> warehouseIds, CancellationToken cancellationToken)
    {
        var pending = await shipmentRepository.Query()
            .Where(shipment => shipment.Status == ShipmentStatus.PendingApproval && warehouseIds.Contains(shipment.SourceWarehouseId))
            .Select(shipment => new { shipment.Id, shipment.SourceWarehouseId })
            .ToListAsync(cancellationToken);
        if (pending.Count == 0) return [];

        var shipmentIds = pending.Select(shipment => shipment.Id).ToArray();
        var items = await shipmentItemRepository.Query()
            .Where(item => shipmentIds.Contains(item.ShipmentId))
            .Select(item => new { item.Id, item.ShipmentId, item.ProductId, item.Quantity })
            .ToListAsync(cancellationToken);
        var reserved = (await reservationRepository.Query()
            .Where(reservation => reservation.SourceType == "Shipment" && reservation.Status == StockReservationStatus.Active &&
                                  reservation.SourceItemId != null && shipmentIds.Contains(reservation.SourceId))
            .Select(reservation => new { reservation.SourceItemId, Open = reservation.Quantity - reservation.ReleasedQuantity })
            .ToListAsync(cancellationToken))
            .GroupBy(reservation => reservation.SourceItemId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(reservation => reservation.Open));
        var warehouseByShipment = pending.ToDictionary(shipment => shipment.Id, shipment => shipment.SourceWarehouseId);

        var shortages = items
            .Select(item => new
            {
                WarehouseId = warehouseByShipment[item.ShipmentId],
                item.ProductId,
                Missing = item.Quantity - reserved.GetValueOrDefault(item.Id)
            })
            .Where(row => row.Missing > 0)
            .GroupBy(row => (row.WarehouseId, row.ProductId))
            .Select(group => new { group.Key.WarehouseId, group.Key.ProductId, Quantity = group.Sum(row => row.Missing) })
            .ToList();
        if (shortages.Count == 0) return [];

        var productIds = shortages.Select(row => row.ProductId).Distinct().ToArray();
        var names = await productRepository.Query()
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, product => product.Name, cancellationToken);
        return shortages
            .Select(row => new WarehouseDashboardBackorderView(
                row.WarehouseId, row.ProductId, names.GetValueOrDefault(row.ProductId, string.Empty), row.Quantity))
            .OrderBy(row => row.ProductName)
            .ToArray();
    }
}
