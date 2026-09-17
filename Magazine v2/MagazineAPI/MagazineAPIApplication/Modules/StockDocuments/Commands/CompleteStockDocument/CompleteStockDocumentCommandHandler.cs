using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class CompleteStockDocumentCommandHandler(
    IStockDocumentRepository documentRepository,
    IRepository<Inventory> inventoryRepository,
    IWarehouseContext warehouseContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CompleteStockDocumentCommand, bool>
{
    public async ValueTask<bool> Handle(
        CompleteStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        await unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
        {
            var document = await documentRepository.GetByIdAsync(
                command.Id,
                trackChanges: true,
                transactionToken)
                ?? throw new ResourceNotFoundException("Dokument nie został znaleziony.");
            if (!warehouseContext.CanAccess(document.WarehouseId))
                throw new ForbiddenOperationException("Brak dostępu do magazynu dokumentu.");
            var expectedStatus = document.Type == StockDocumentType.Receipt
                ? StockDocumentStatus.Received
                : StockDocumentStatus.Draft;
            if (document.Status != expectedStatus)
            {
                throw new ResourceConflictException(
                    document.Type == StockDocumentType.Receipt
                        ? "Przesyłka PZ musi najpierw zostać przyjęta przez pracownika."
                        : "Dokument został już zatwierdzony.");
            }
            if (document.Items.Count == 0)
                throw new CommandValidationException("Dokument nie zawiera pozycji.");

            var approvingUserId = warehouseContext.UserId
                ?? throw new ForbiddenOperationException("Brak identyfikatora użytkownika zatwierdzającego.");

            foreach (var item in document.Items)
                await ApplyItem(document.Type, item, transactionToken);

            document.Status = StockDocumentStatus.Completed;
            document.CompletedAtUtc = DateTime.UtcNow;
            document.ApprovedByUserId = approvingUserId;
            await documentRepository.SaveChangesAsync(transactionToken);
        }, cancellationToken);

        return true;
    }

    private async Task ApplyItem(
        StockDocumentType type,
        StockDocumentItem item,
        CancellationToken cancellationToken)
    {
        var inventory = await inventoryRepository.FirstOrDefaultAsync(
            value => value.ProductId == item.ProductId
                && value.LocationId == item.LocationId,
            cancellationToken);

        if (type == StockDocumentType.Receipt)
        {
            if (inventory is null)
            {
                await inventoryRepository.AddAsync(new Inventory
                {
                    InventoryId = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    LocationId = item.LocationId,
                    Quantity = item.Quantity,
                    ReservedQuantity = 0
                }, cancellationToken);
                return;
            }

            inventory.Quantity += item.Quantity;
            await inventoryRepository.UpdateAsync(inventory, cancellationToken);
            return;
        }

        if (inventory is null || inventory.Quantity - inventory.ReservedQuantity < item.Quantity)
        {
            throw new ResourceConflictException(
                $"Brak dostępnego stanu produktu {item.Product.Name} w lokalizacji {item.Location.LocationCode}.");
        }

        inventory.Quantity -= item.Quantity;
        await inventoryRepository.UpdateAsync(inventory, cancellationToken);
    }
}
