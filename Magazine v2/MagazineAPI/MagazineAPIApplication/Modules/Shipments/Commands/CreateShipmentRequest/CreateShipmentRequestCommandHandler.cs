using MagazineAPIApplication.Common;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class CreateShipmentRequestCommandHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<Product> productRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<UnitOfMeasure> unitRepository,
    IRepository<Location> locationRepository,
    IRepository<Inventory> inventoryRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IDocumentNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    IShipmentReservationService shipmentReservationService,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<CreateShipmentRequestCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateShipmentRequestCommand command, CancellationToken cancellationToken)
    {
        var destinationWarehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn, dla którego składasz prośbę.");

        if (command.Items.Count == 0)
            throw new CommandValidationException("Dodaj co najmniej jedną pozycję zapotrzebowania.");

        var destinationWarehouse = await warehouseRepository.FirstOrDefaultAsync(
            warehouse => warehouse.Id == destinationWarehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn docelowy nie istnieje.");

        var requestedItems = new List<RequestedShipmentItem>();
        foreach (var item in command.Items)
        {
            var product = await ResolveProductAsync(item, cancellationToken);
            QuantityRules.Ensure(item.Quantity, (await unitRepository.FirstOrDefaultAsync(unit => unit.Id == product.UnitOfMeasureId, cancellationToken))?.Symbol);
            requestedItems.Add(new RequestedShipmentItem(
                product,
                ResolveShipmentBarcode(product, item.Barcode),
                item.Quantity));
        }

        requestedItems = requestedItems
            .GroupBy(item => item.Product.Id)
            .Select(group =>
            {
                var first = group.First();
                return new RequestedShipmentItem(
                    first.Product,
                    first.Barcode,
                    group.Sum(item => item.Quantity));
            })
            .ToList();

        var sourceWarehouse = await FindSourceWarehouseAsync(
            destinationWarehouse.Id,
            requestedItems,
            cancellationToken);
        var sourceIsHub = sourceWarehouse.IsHub;

        var now = DateTime.UtcNow;
        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            Number = await numberGenerator.GenerateAsync(destinationWarehouse.Id, "REQ", now, cancellationToken),
            SourceWarehouseId = sourceWarehouse.Id,
            DestinationWarehouseId = destinationWarehouse.Id,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = now,
            Status = ShipmentStatus.PendingApproval,
            Items = requestedItems
                .Select(item => new ShipmentItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.Product.Id,
                    Barcode = item.Barcode,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await shipmentRepository.AddAsync(shipment, ct);
            await shipmentReservationService.ReserveAsync(
                shipment,
                shipment.Items.ToArray(),
                "Rezerwacja towaru pod zapotrzebowanie.",
                ct,
                allowPartial: sourceIsHub);
            await auditLogWriter.RecordAsync(
                new AuditLogRecord(
                    "Create",
                    nameof(Shipment),
                    shipment.Id.ToString(),
                    $"Utworzono zapotrzebowanie {shipment.Number}."),
                ct);
        }, cancellationToken);

        await publisher.Publish(new ShipmentRequestedEvent(
            shipment.Id,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            currentUser.UserId,
            shipment.Number,
            shipment.Items.Count), cancellationToken);

        return shipment.Id;
    }

    private async Task<Warehouse> FindSourceWarehouseAsync(
        Guid destinationWarehouseId,
        IReadOnlyList<RequestedShipmentItem> requestedItems,
        CancellationToken cancellationToken)
    {
        var allWarehouses = await warehouseRepository.AllAsync(cancellationToken);
        var hub = allWarehouses.FirstOrDefault(warehouse => warehouse.IsHub);
        var warehouses = allWarehouses
            .Where(warehouse => warehouse.Id != destinationWarehouseId)
            .Where(warehouse => hub is null || hub.Id == destinationWarehouseId || warehouse.Id == hub.Id)
            .OrderBy(warehouse => warehouse.Name)
            .ToArray();
        var locations = await locationRepository.AllAsync(cancellationToken);
        var inventories = await inventoryRepository.AllAsync(cancellationToken);

        var productIds = requestedItems
            .Select(item => item.Product.Id)
            .ToHashSet();
        var warehouseByLocationId = locations.ToDictionary(
            location => location.Id,
            location => location.WarehouseId);

        var availableByWarehouseAndProduct = inventories
            .Where(inventory => productIds.Contains(inventory.ProductId))
            .Where(inventory => warehouseByLocationId.ContainsKey(inventory.LocationId))
            .GroupBy(inventory => new
            {
                WarehouseId = warehouseByLocationId[inventory.LocationId],
                inventory.ProductId
            })
            .ToDictionary(
                group => (group.Key.WarehouseId, group.Key.ProductId),
                group => group.Sum(inventory => inventory.AvailableQuantity));

        if (hub is not null && hub.Id != destinationWarehouseId)
            return hub;

        var sourceWarehouse = warehouses
            .Select(warehouse => new
            {
                Warehouse = warehouse,
                TotalAvailable = requestedItems.Sum(item =>
                    availableByWarehouseAndProduct.GetValueOrDefault(
                        (warehouse.Id, item.Product.Id)))
            })
            .Where(candidate => requestedItems.All(item =>
                availableByWarehouseAndProduct.GetValueOrDefault(
                    (candidate.Warehouse.Id, item.Product.Id)) >= item.Quantity))
            .OrderByDescending(candidate => candidate.TotalAvailable)
            .ThenBy(candidate => candidate.Warehouse.Name)
            .Select(candidate => candidate.Warehouse)
            .FirstOrDefault();

        return sourceWarehouse
            ?? throw new CommandValidationException(hub is not null && hub.Id != destinationWarehouseId
                ? $"Magazyn przeładunkowy „{hub.Name}” nie ma pełnego zapotrzebowania. Zmniejsz ilości albo zamów towar w magazynie przeładunkowym."
                : "Nie znaleziono magazynu z pełnym zapotrzebowaniem dla tej prośby.");
    }

    private async Task<Product> ResolveProductAsync(
        CreateShipmentItem item,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(item.Barcode) &&
            item.ProductId is { } selectedProductId &&
            selectedProductId != Guid.Empty)
            throw new CommandValidationException("Podaj kod kreskowy albo wybierz produkt, nie oba naraz.");

        if (item.ProductId is { } productId && productId != Guid.Empty)
        {
            return await productRepository.FirstOrDefaultAsync(
                candidate => candidate.Id == productId && candidate.IsActive,
                cancellationToken)
                ?? throw new CommandValidationException("Nie znaleziono aktywnego produktu.");
        }

        var barcode = NormalizeBarcode(item.Barcode);
        if (string.IsNullOrWhiteSpace(barcode))
            throw new CommandValidationException("Podaj kod kreskowy albo wybierz produkt.");

        return await productRepository.FirstOrDefaultAsync(
            candidate => candidate.Barcode == barcode && candidate.IsActive,
            cancellationToken)
            ?? throw new CommandValidationException($"Nie znaleziono aktywnego produktu dla kodu {barcode}.");
    }

    private static string ResolveShipmentBarcode(Product product, string? barcode)
    {
        if (!string.IsNullOrWhiteSpace(product.Barcode)) return product.Barcode;
        if (!string.IsNullOrWhiteSpace(barcode)) return barcode.Trim();
        return product.Sku;
    }

    private static string NormalizeBarcode(string? barcode) => barcode?.Trim() ?? string.Empty;

    private sealed record RequestedShipmentItem(Product Product, string Barcode, decimal Quantity);
}
