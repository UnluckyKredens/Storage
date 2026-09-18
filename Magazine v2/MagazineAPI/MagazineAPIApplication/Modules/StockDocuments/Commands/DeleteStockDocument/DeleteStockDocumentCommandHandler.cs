using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class DeleteStockDocumentCommandHandler(
    IStockDocumentRepository repository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<DeleteStockDocumentCommand, bool>
{
    public async ValueTask<bool> Handle(
        DeleteStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(
            command.Id,
            trackChanges: true,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Dokument nie został znaleziony.");
        if (!warehouseContext.CanAccess(document.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu dokumentu.");
        if (document.Status != StockDocumentStatus.Draft)
            throw new ResourceConflictException("Można usunąć tylko dokument w statusie Szkic.");

        await repository.DeleteAsync(document, cancellationToken);
        return true;
    }
}
