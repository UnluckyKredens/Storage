using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed class GetPurchaseOrderQueryHandler(
    IRepository<PurchaseOrder> orderRepository,
    IRepository<PurchaseOrderItem> orderItemRepository,
    IRepository<Product> productRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<Contractor> contractorRepository,
    IRepository<User> userRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetPurchaseOrderQuery, PurchaseOrderView>
{
    public async ValueTask<PurchaseOrderView> Handle(
        GetPurchaseOrderQuery query,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.FirstOrDefaultAsync(
            item => item.Id == query.PurchaseOrderId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Zamówienie zewnętrzne nie zostało znalezione.");

        if (!warehouseContext.CanAccess(order.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do zamówienia z innego magazynu.");

        order.Items = (await orderItemRepository.FilterByAsync(
                item => item.PurchaseOrderId == order.Id,
                cancellationToken))
            .ToList();
        var products = (await productRepository.AllAsync(cancellationToken))
            .ToDictionary(product => product.Id);
        var warehouses = (await warehouseRepository.AllAsync(cancellationToken))
            .ToDictionary(warehouse => warehouse.Id);
        var contractors = (await contractorRepository.AllAsync(cancellationToken))
            .ToDictionary(contractor => contractor.Id);

        var userIds = PurchaseOrderMapper.UserIds([order]);
        var users = (await userRepository.FilterByAsync(user => userIds.Contains(user.Id), cancellationToken))
            .ToDictionary(user => user.Id);

        return PurchaseOrderMapper.ToView(order, warehouses, contractors, products, users);
    }
}
