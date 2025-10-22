using MagazineAPIApplication.Common;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.PurchaseOrders;

public sealed class CreatePurchaseOrderCommandHandler(
    IRepository<PurchaseOrder> orderRepository,
    IRepository<Product> productRepository,
    IRepository<Contractor> contractorRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<UnitOfMeasure> unitRepository,
    IWarehouseContext warehouseContext,
    ICurrentUser currentUser,
    IDocumentNumberGenerator numberGenerator,
    IAuditLogWriter auditLogWriter) : ICommandHandler<CreatePurchaseOrderCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var warehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn dla zamówienia.");
        if (command.ContractorId == Guid.Empty)
            throw new CommandValidationException("Wybierz dostawcę.");
        if (command.Items.Count == 0)
            throw new CommandValidationException("Dodaj co najmniej jedną pozycję zamówienia.");

        var warehouse = await warehouseRepository.FirstOrDefaultAsync(
            item => item.Id == warehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn nie istnieje.");
        if (!warehouseContext.CanAccess(warehouse.Id))
            throw new ForbiddenOperationException("Brak dostępu do magazynu zamówienia.");
        var hub = await warehouseRepository.FirstOrDefaultAsync(item => item.IsHub, cancellationToken);
        if (hub is not null && hub.Id != warehouse.Id)
            throw new CommandValidationException(
                $"Zamówienia zewnętrzne przyjmuje magazyn przeładunkowy „{hub.Name}”. Pozostałe magazyny składają prośbę o wysyłkę.");

        var contractor = await contractorRepository.FirstOrDefaultAsync(
            item => item.Id == command.ContractorId,
            cancellationToken)
            ?? throw new CommandValidationException("Dostawca nie istnieje.");
        if (contractor.Type is not (ContractorType.Supplier or ContractorType.Both))
            throw new CommandValidationException("Wybrany kontrahent nie jest dostawcą.");

        var requestedItems = new List<PurchaseOrderItem>();
        foreach (var item in command.Items)
        {
            if (item.ProductId == Guid.Empty)
                throw new CommandValidationException("Wybierz produkt.");
            if (item.UnitPrice is < 0)
                throw new CommandValidationException("Cena zakupu nie może być ujemna.");

            var product = await productRepository.FirstOrDefaultAsync(
                candidate => candidate.Id == item.ProductId && candidate.IsActive,
                cancellationToken)
                ?? throw new CommandValidationException("Nie znaleziono aktywnego produktu.");
            QuantityRules.Ensure(item.Quantity, (await unitRepository.FirstOrDefaultAsync(unit => unit.Id == product.UnitOfMeasureId, cancellationToken))?.Symbol);

            requestedItems.Add(new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Barcode = product.Barcode ?? product.Sku,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice ?? product.PurchasePrice
            });
        }

        var items = requestedItems
            .GroupBy(item => new { item.ProductId, item.UnitPrice })
            .Select(group =>
            {
                var first = group.First();
                var quantity = group.Sum(item => item.Quantity);
                var total = group.Sum(item => item.Quantity * item.UnitPrice);
                return new PurchaseOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = first.ProductId,
                    Barcode = first.Barcode,
                    Quantity = quantity,
                    UnitPrice = first.UnitPrice
                };
            })
            .ToList();

        var now = DateTime.UtcNow;
        var order = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            Number = await numberGenerator.GenerateAsync(warehouse.Id, "ZZ", now, cancellationToken),
            WarehouseId = warehouse.Id,
            ContractorId = contractor.Id,
            CreatedByUserId = currentUser.UserId,
            CreatedOnUtc = now,
            Status = PurchaseOrderStatus.PendingApproval,
            Notes = command.Notes?.Trim(),
            Items = items
        };

        await orderRepository.AddAsync(order, cancellationToken);
        await auditLogWriter.RecordAsync(
            new AuditLogRecord(
                "Create",
                nameof(PurchaseOrder),
                order.Id.ToString(),
                $"Utworzono zamówienie zewnętrzne {order.Number}."),
            cancellationToken);

        return order.Id;
    }
}
