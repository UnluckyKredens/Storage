using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class GetShipmentQueryHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IRepository<Product> productRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetShipmentQuery, ShipmentView>
{
    public async ValueTask<ShipmentView> Handle(GetShipmentQuery query, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepository.FirstOrDefaultAsync(
            item => item.ShipmentId == query.ShipmentId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (!warehouseContext.CanAccess(shipment.SourceWarehouseId) &&
            !warehouseContext.CanAccess(shipment.DestinationWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wysyłki.");

        shipment.Items = (await shipmentItemRepository.FilterByAsync(
                item => item.ShipmentId == shipment.ShipmentId,
                cancellationToken))
            .ToList();
        var products = (await productRepository.AllAsync(cancellationToken))
            .ToDictionary(product => product.ProductId);
        var warehouses = (await warehouseRepository.AllAsync(cancellationToken))
            .ToDictionary(warehouse => warehouse.WarehouseId);

        return ShipmentMapper.ToView(shipment, warehouses, products);
    }
}
