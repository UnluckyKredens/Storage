using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class GetStockDocumentQueryHandler(
    IStockDocumentRepository repository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetStockDocumentQuery, StockDocumentDetailsView>
{
    public async ValueTask<StockDocumentDetailsView> Handle(
        GetStockDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(query.Id, cancellationToken: cancellationToken)
            ?? throw new ResourceNotFoundException("Dokument nie został znaleziony.");
        if (!warehouseContext.CanAccess(document.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu dokumentu.");

        return StockDocumentMapper.ToDetailsView(document);
    }
}
