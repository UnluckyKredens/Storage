using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed class GetPurchaseOrdersQueryHandler(
    IRepository<PurchaseOrder> orderRepository,
    IRepository<PurchaseOrderItem> orderItemRepository,
    IRepository<Product> productRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<Contractor> contractorRepository,
    IRepository<User> userRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetPurchaseOrdersQuery, IReadOnlyList<PurchaseOrderView>>
{
    public async ValueTask<IReadOnlyList<PurchaseOrderView>> Handle(
        GetPurchaseOrdersQuery query,
        CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var ordersQuery = orderRepository.Query();
        if (warehouseContext.WarehouseId is { } activeWarehouseId)
            ordersQuery = ordersQuery.Where(order => order.WarehouseId == activeWarehouseId);
        if (query.Status is { } status)
            ordersQuery = ordersQuery.Where(order => order.Status == status);

        var orders = await ordersQuery
            .OrderByDescending(order => order.CreatedOnUtc)
            .Take(500)
            .ToArrayAsync(cancellationToken);
        if (orders.Length == 0) return [];

        var orderIds = orders.Select(order => order.Id).ToHashSet();
        var items = (await orderItemRepository.Query()
                .Where(item => orderIds.Contains(item.PurchaseOrderId))
                .ToListAsync(cancellationToken))
            .ToLookup(item => item.PurchaseOrderId);

        var productIds = items.SelectMany(group => group).Select(item => item.ProductId).ToHashSet();
        var warehouseIds = orders.Select(order => order.WarehouseId).ToHashSet();
        var contractorIds = orders.Select(order => order.ContractorId).ToHashSet();
        var products = await productRepository.Query()
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);
        var warehouses = await warehouseRepository.Query()
            .Where(warehouse => warehouseIds.Contains(warehouse.Id))
            .ToDictionaryAsync(warehouse => warehouse.Id, cancellationToken);
        var contractors = await contractorRepository.Query()
            .Where(contractor => contractorIds.Contains(contractor.Id))
            .ToDictionaryAsync(contractor => contractor.Id, cancellationToken);

        var userIds = PurchaseOrderMapper.UserIds(orders);
        var users = await userRepository.Query()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        return orders
            .Select(order =>
            {
                order.Items = items[order.Id].ToList();
                return PurchaseOrderMapper.ToView(order, warehouses, contractors, products, users);
            })
            .ToArray();
    }
}
