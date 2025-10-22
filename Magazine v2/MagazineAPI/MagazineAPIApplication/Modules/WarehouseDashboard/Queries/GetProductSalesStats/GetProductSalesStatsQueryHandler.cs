using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.WarehouseDashboard;

public sealed class GetProductSalesStatsQueryHandler(
    IRepository<StorefrontOrder> orderRepository,
    IRepository<Inventory> inventoryRepository,
    IRepository<Location> locationRepository,
    IRepository<Product> productRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetProductSalesStatsQuery, ProductSalesStatsView>
{
    public const int MaxDays = 365;
    private const int ListSize = 5;

    public async ValueTask<ProductSalesStatsView> Handle(GetProductSalesStatsQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var days = Math.Clamp(query.Days, 1, MaxDays);
        var toUtc = DateTime.UtcNow;
        var fromUtc = DateOnly.FromDateTime(toUtc).AddDays(-(days - 1)).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var warehouseId = warehouseContext.WarehouseId;

        var orders = orderRepository.Query()
            .Where(order => (order.Status == StorefrontOrderStatus.Completed || order.Status == StorefrontOrderStatus.Shipped)
                && order.CompletedOnUtc >= fromUtc);
        if (warehouseId is not null)
            orders = orders.Where(order => order.AssignedWarehouseId == warehouseId);

        var sold = await orders
            .SelectMany(order => order.Items, (order, item) => new
            {
                OrderId = order.Id, item.ProductId, item.Quantity, Value = item.Quantity * item.UnitPrice
            })
            .ToListAsync(cancellationToken);

        var locations = locationRepository.Query();
        if (warehouseId is not null)
            locations = locations.Where(location => location.WarehouseId == warehouseId);
        var stock = await inventoryRepository.Query()
            .Where(item => locations.Any(location => location.Id == item.LocationId))
            .GroupBy(item => item.ProductId)
            .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
            .ToDictionaryAsync(row => row.ProductId, row => row.Quantity, cancellationToken);

        var soldByProduct = sold
            .GroupBy(row => row.ProductId)
            .ToDictionary(
                group => group.Key,
                group => (Quantity: group.Sum(row => row.Quantity), Revenue: group.Sum(row => row.Value),
                          Orders: group.Select(row => row.OrderId).Distinct().Count()));

        var candidateIds = soldByProduct.Keys.Union(stock.Where(row => row.Value > 0).Select(row => row.Key)).ToArray();
        var products = await productRepository.Query()
            .Where(product => candidateIds.Contains(product.Id) && (product.IsActive || soldByProduct.Keys.Contains(product.Id)))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        var rows = products.Values
            .Select(product =>
            {
                var sales = soldByProduct.GetValueOrDefault(product.Id);
                return new ProductSalesView(
                    product.Id, product.Name, product.Sku, sales.Quantity, sales.Revenue, sales.Orders,
                    stock.GetValueOrDefault(product.Id));
            })
            .ToList();

        var best = rows.Where(row => row.SoldQuantity > 0)
            .OrderByDescending(row => row.SoldQuantity).ThenByDescending(row => row.Revenue).ThenBy(row => row.ProductName)
            .Take(ListSize).ToList();
        var bestIds = best.Select(row => row.ProductId).ToHashSet();
        var worst = rows.Where(row => !bestIds.Contains(row.ProductId))
            .OrderBy(row => row.SoldQuantity).ThenByDescending(row => row.StockQuantity).ThenBy(row => row.ProductName)
            .Take(ListSize).ToList();

        return new ProductSalesStatsView(
            days, fromUtc, toUtc, soldByProduct.Count == 0 ? 0 : sold.Select(row => row.OrderId).Distinct().Count(),
            sold.Sum(row => row.Quantity), sold.Sum(row => row.Value), best, worst);
    }
}
