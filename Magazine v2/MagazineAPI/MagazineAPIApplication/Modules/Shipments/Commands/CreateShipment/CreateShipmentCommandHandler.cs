using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPIEvent.Request;
using Mediator;

namespace MagazineAPIApplication.Modules.Shipments;

public sealed class CreateShipmentCommandHandler(
    IRepository<Shipment> shipmentRepository,
    IRepository<Product> productRepository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IDocumentNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    IShipmentReservationService shipmentReservationService,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ICommandHandler<CreateShipmentCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        var sourceWarehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn źródłowy.");

        if (command.DestinationWarehouseId == Guid.Empty)
            throw new CommandValidationException("Wybierz magazyn docelowy.");
        if (command.DestinationWarehouseId == sourceWarehouseId)
            throw new CommandValidationException("Magazyn docelowy musi być inny niż źródłowy.");
        if (command.Items.Count == 0)
            throw new CommandValidationException("Dodaj co najmniej jedną pozycję wysyłki.");

        var sourceWarehouse = await warehouseRepository.FirstOrDefaultAsync(
            warehouse => warehouse.WarehouseId == sourceWarehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn źródłowy nie istnieje.");
        var destinationWarehouse = await warehouseRepository.FirstOrDefaultAsync(
            warehouse => warehouse.WarehouseId == command.DestinationWarehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn docelowy nie istnieje.");

        if (!warehouseContext.CanAccess(sourceWarehouse.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu źródłowego.");

        var requestedItems = new List<ResolvedShipmentItem>();
        foreach (var item in command.Items)
        {
            if (item.Quantity <= 0)
                throw new CommandValidationException("Ilość musi być większa od zera.");

            var product = await ResolveProductAsync(item, cancellationToken);
            requestedItems.Add(new ResolvedShipmentItem(
                product,
                ResolveShipmentBarcode(product, item.Barcode),
                item.Quantity));
        }

        var items = requestedItems
            .GroupBy(item => item.Product.ProductId)
            .Select(group =>
            {
                var first = group.First();
                return new ShipmentItem
                {
                    ShipmentItemId = Guid.NewGuid(),
                    ProductId = first.Product.ProductId,
                    Barcode = first.Barcode,
                    Quantity = group.Sum(item => item.Quantity)
                };
            })
            .ToList();

        var now = DateTime.UtcNow;
        var shipment = new Shipment
        {
            ShipmentId = Guid.NewGuid(),
            Number = await numberGenerator.GenerateAsync(sourceWarehouse.WarehouseId, "WZ", now, cancellationToken),
            SourceWarehouseId = sourceWarehouse.WarehouseId,
            DestinationWarehouseId = destinationWarehouse.WarehouseId,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = now,
            Status = ShipmentStatus.PendingApproval,
            Items = items
        };

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await shipmentRepository.AddAsync(shipment, ct);
            await shipmentReservationService.ReserveAsync(
                shipment,
                items,
                "Rezerwacja towaru pod wysyłkę.",
                ct);
            await auditLogWriter.RecordAsync(
                new AuditLogRecord(
                    "Create",
                    nameof(Shipment),
                    shipment.ShipmentId.ToString(),
                    $"Utworzono wysyłkę {shipment.Number}."),
                ct);
        }, cancellationToken);

        await publisher.Publish(new ShipmentCreatedEvent(
            shipment.ShipmentId,
            shipment.SourceWarehouseId,
            shipment.DestinationWarehouseId,
            currentUser.UserId,
            shipment.Number,
            shipment.Items.Count), cancellationToken);

        return shipment.ShipmentId;
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
                candidate => candidate.ProductId == productId && candidate.IsActive,
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

    private sealed record ResolvedShipmentItem(Product Product, string Barcode, decimal Quantity);
}
