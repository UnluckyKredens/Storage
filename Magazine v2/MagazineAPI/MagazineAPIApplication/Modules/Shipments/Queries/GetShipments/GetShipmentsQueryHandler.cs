using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class GetShipmentsQueryHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<ShipmentItem> shipmentItemRepository,
    IRepository<Product> productRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext) : IQueryHandler<GetShipmentsQuery, IReadOnlyList<ShipmentView>>
{
    public async ValueTask<IReadOnlyList<ShipmentView>> Handle(GetShipmentsQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var shipmentsQuery = shipmentRepository.Query();
        if (warehouseContext.WarehouseId is { } activeWarehouseId)
        {
            shipmentsQuery = shipmentsQuery.Where(shipment =>
                shipment.SourceWarehouseId == activeWarehouseId ||
                shipment.DestinationWarehouseId == activeWarehouseId);
        }

        if (query.ReadyOnly)
        {
            shipmentsQuery = shipmentsQuery
                .Where(shipment => shipment.Status == ShipmentStatus.Sent ||
                                   shipment.Status == ShipmentStatus.InTransit);
        }
        else if (query.Status is { } status)
        {
            shipmentsQuery = shipmentsQuery.Where(shipment => shipment.Status == status);
        }

        var shipments = await shipmentsQuery
            .OrderByDescending(shipment => shipment.CreatedOnUtc)
            .Take(500)
            .ToArrayAsync(cancellationToken);
        if (shipments.Length == 0) return [];

        var shipmentIds = shipments.Select(shipment => shipment.ShipmentId).ToHashSet();
        var items = (await shipmentItemRepository.Query()
            .Where(item => shipmentIds.Contains(item.ShipmentId))
            .ToListAsync(cancellationToken))
            .ToLookup(item => item.ShipmentId);
        var productIds = items.SelectMany(group => group)
            .Select(item => item.ProductId)
            .ToHashSet();
        var warehouseIds = shipments
            .SelectMany(shipment => new[] { shipment.SourceWarehouseId, shipment.DestinationWarehouseId })
            .ToHashSet();
        var products = await productRepository.Query()
            .Where(product => productIds.Contains(product.ProductId))
            .ToDictionaryAsync(product => product.ProductId, cancellationToken);
        var warehouses = await warehouseRepository.Query()
            .Where(warehouse => warehouseIds.Contains(warehouse.WarehouseId))
            .ToDictionaryAsync(warehouse => warehouse.WarehouseId, cancellationToken);

        return shipments
            .Select(shipment =>
            {
                shipment.Items = items[shipment.ShipmentId].ToList();
                return ShipmentMapper.ToView(shipment, warehouses, products);
            })
            .ToArray();
    }
}
