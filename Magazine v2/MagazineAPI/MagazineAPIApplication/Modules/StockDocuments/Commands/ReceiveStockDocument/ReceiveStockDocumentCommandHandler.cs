using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class ReceiveStockDocumentCommandHandler(
    IStockDocumentRepository repository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<ReceiveStockDocumentCommand, bool>
{
    public async ValueTask<bool> Handle(
        ReceiveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(
            command.Id,
            trackChanges: true,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Przesyłka nie została znaleziona.");

        if (!warehouseContext.CanAccess(document.WarehouseId))
            throw new ForbiddenOperationException("Przesyłka należy do innego magazynu.");
        if (document.Type != StockDocumentType.Receipt)
            throw new CommandValidationException("Można przyjąć tylko przesyłkę PZ.");
        if (document.Status != StockDocumentStatus.Draft)
            throw new ResourceConflictException("Przesyłka została już przyjęta.");

        document.Status = StockDocumentStatus.Received;
        document.ReceivedAtUtc = DateTime.UtcNow;
        document.ReceivedByUserId = warehouseContext.UserId
            ?? throw new ForbiddenOperationException("Brak identyfikatora użytkownika przyjmującego.");

        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
