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
            item => item.WarehouseId == warehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn nie istnieje.");
        if (!warehouseContext.CanAccess(warehouse.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu zamówienia.");

        var contractor = await contractorRepository.FirstOrDefaultAsync(
            item => item.ContractorId == command.ContractorId,
            cancellationToken)
            ?? throw new CommandValidationException("Dostawca nie istnieje.");
        if (contractor.Type is not (ContractorType.Supplier or ContractorType.Both))
            throw new CommandValidationException("Wybrany kontrahent nie jest dostawcą.");

        var requestedItems = new List<PurchaseOrderItem>();
        foreach (var item in command.Items)
        {
            if (item.ProductId == Guid.Empty)
                throw new CommandValidationException("Wybierz produkt.");
            if (item.Quantity <= 0)
                throw new CommandValidationException("Ilość musi być większa od zera.");
            if (item.UnitPrice is < 0)
                throw new CommandValidationException("Cena zakupu nie może być ujemna.");

            var product = await productRepository.FirstOrDefaultAsync(
                candidate => candidate.ProductId == item.ProductId && candidate.IsActive,
                cancellationToken)
                ?? throw new CommandValidationException("Nie znaleziono aktywnego produktu.");

            requestedItems.Add(new PurchaseOrderItem
            {
                PurchaseOrderItemId = Guid.NewGuid(),
                ProductId = product.ProductId,
                Barcode = product.Barcode ?? product.Sku,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice ?? product.PurchasePrice
            });
        }

        var items = requestedItems
            .GroupBy(item => item.ProductId)
            .Select(group =>
            {
                var first = group.First();
                var quantity = group.Sum(item => item.Quantity);
                var total = group.Sum(item => item.Quantity * item.UnitPrice);
                return new PurchaseOrderItem
                {
                    PurchaseOrderItemId = Guid.NewGuid(),
                    ProductId = first.ProductId,
                    Barcode = first.Barcode,
                    Quantity = quantity,
                    UnitPrice = total / quantity
                };
            })
            .ToList();

        var now = DateTime.UtcNow;
        var order = new PurchaseOrder
        {
            PurchaseOrderId = Guid.NewGuid(),
            Number = await numberGenerator.GenerateAsync(warehouse.WarehouseId, "ZZ", now, cancellationToken),
            WarehouseId = warehouse.WarehouseId,
            ContractorId = contractor.ContractorId,
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
                order.PurchaseOrderId.ToString(),
                $"Utworzono zamówienie zewnętrzne {order.Number}."),
            cancellationToken);

        return order.PurchaseOrderId;
    }
}
