using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Entities.History;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.WarehouseDashboard;

/// <summary>Stock and shipment flow statistics built from the history database.</summary>
public sealed class GetWarehouseFlowStatsQueryHandler(
    IRepository<InventoryHistory> inventoryHistoryRepository,
    IRepository<ShipmentHistory> shipmentHistoryRepository,
    IRepository<Location> locationRepository,
    IRepository<Product> productRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetWarehouseFlowStatsQuery, WarehouseFlowStatsView>
{
    public const int MaxDays = 90;
    private const int TopProductCount = 5;

    public async ValueTask<WarehouseFlowStatsView> Handle(GetWarehouseFlowStatsQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var days = Math.Clamp(query.Days, 1, MaxDays);
        var toUtc = DateTime.UtcNow;
        var fromDate = DateOnly.FromDateTime(toUtc).AddDays(-(days - 1));
        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var locationsQuery = locationRepository.Query();
        if (warehouseContext.WarehouseId is { } activeWarehouseId)
            locationsQuery = locationsQuery.Where(location => location.WarehouseId == activeWarehouseId);
        var locations = await locationsQuery
            .Select(location => new { LocationId = location.Id, location.WarehouseId })
            .ToListAsync(cancellationToken);
        var locationIds = locations.Select(location => location.LocationId).ToArray();
        var warehouseIds = locations.Select(location => location.WarehouseId).Distinct().ToArray();

        // Rows with a zero change are reservations, which do not move stock.
        var changes = await inventoryHistoryRepository.Query()
            .Where(item => item.CreatedOn >= fromUtc && item.QuantityChange != 0 && locationIds.Contains(item.LocationId))
            .Select(item => new { item.CreatedOn, item.ProductId, item.QuantityChange })
            .ToListAsync(cancellationToken);

        var daily = Enumerable.Range(0, days)
            .Select(offset => fromDate.AddDays(offset))
            .Select(date =>
            {
                var dayChanges = changes.Where(change => DateOnly.FromDateTime(change.CreatedOn) == date).ToList();
                return new WarehouseFlowDayView(
                    date,
                    dayChanges.Where(change => change.QuantityChange > 0).Sum(change => change.QuantityChange),
                    dayChanges.Where(change => change.QuantityChange < 0).Sum(change => -change.QuantityChange));
            })
            .ToArray();

        var topProductRows = changes
            .GroupBy(change => change.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Inbound = group.Where(change => change.QuantityChange > 0).Sum(change => change.QuantityChange),
                Outbound = group.Where(change => change.QuantityChange < 0).Sum(change => -change.QuantityChange)
            })
            .OrderByDescending(row => row.Inbound + row.Outbound)
            .Take(TopProductCount)
            .ToList();
        var topProductIds = topProductRows.Select(row => row.ProductId).ToArray();
        var productNames = await productRepository.Query()
            .Where(product => topProductIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, product => product.Name, cancellationToken);
        var topProducts = topProductRows
            .Select(row => new WarehouseFlowProductView(
                row.ProductId, productNames.GetValueOrDefault(row.ProductId, string.Empty), row.Inbound, row.Outbound))
            .ToArray();

        var inbound = daily.Sum(day => day.Inbound);
        var outbound = daily.Sum(day => day.Outbound);

        return new WarehouseFlowStatsView(
            days, fromUtc, toUtc, inbound, outbound, inbound - outbound, changes.Count, daily, topProducts,
            await BuildShipmentFlowAsync(warehouseIds, fromUtc, cancellationToken));
    }

    private async Task<WarehouseShipmentFlowView> BuildShipmentFlowAsync(
        Guid[] warehouseIds, DateTime fromUtc, CancellationToken cancellationToken)
    {
        var events = await shipmentHistoryRepository.Query()
            .Where(item => item.CreatedOnUtc >= fromUtc &&
                (warehouseIds.Contains(item.SourceWarehouseId) || warehouseIds.Contains(item.DestinationWarehouseId)))
            .Select(item => new { item.ShipmentId, item.EventType, item.CreatedOnUtc })
            .ToListAsync(cancellationToken);

        int Count(string eventType) => events.Count(item => item.EventType == eventType);

        // Durations are measured per shipment from its first event (created or requested).
        var approvalHours = new List<double>();
        var deliveryHours = new List<double>();
        foreach (var shipment in events.GroupBy(item => item.ShipmentId))
        {
            var start = shipment
                .Where(item => item.EventType is "created" or "requested")
                .Select(item => (DateTime?)item.CreatedOnUtc)
                .Min();
            if (start is null) continue;

            var approved = shipment.Where(item => item.EventType == "approved").Select(item => (DateTime?)item.CreatedOnUtc).Min();
            var received = shipment.Where(item => item.EventType == "received").Select(item => (DateTime?)item.CreatedOnUtc).Min();
            if (approved is not null) approvalHours.Add((approved.Value - start.Value).TotalHours);
            if (received is not null) deliveryHours.Add((received.Value - start.Value).TotalHours);
        }

        return new WarehouseShipmentFlowView(
            Count("created"), Count("requested"), Count("approved"), Count("in_transit"), Count("received"), Count("cancelled"),
            approvalHours.Count == 0 ? null : Math.Round(approvalHours.Average(), 1),
            deliveryHours.Count == 0 ? null : Math.Round(deliveryHours.Average(), 1));
    }
}
