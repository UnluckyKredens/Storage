using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class GetShipmentByIdentifierQueryHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IRepository<Product> productRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetShipmentByIdentifierQuery, ShipmentView>
{
    public async ValueTask<ShipmentView> Handle(
        GetShipmentByIdentifierQuery query,
        CancellationToken cancellationToken)
    {
        var identifier = query.Identifier.Trim();
        if (string.IsNullOrWhiteSpace(identifier))
            throw new CommandValidationException("Zeskanuj albo wpisz identyfikator wysyłki.");

        var shipment = Guid.TryParse(identifier, out var shipmentId)
            ? await shipmentRepository.FirstOrDefaultAsync(
                item => item.ShipmentId == shipmentId,
                cancellationToken)
            : await shipmentRepository.FirstOrDefaultAsync(
                item => item.Number == identifier,
                cancellationToken);

        if (shipment is null)
            throw new ResourceNotFoundException("Wysyłka nie została znaleziona.");

        if (!warehouseContext.CanAccess(shipment.DestinationWarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu docelowego tej wysyłki.");

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
