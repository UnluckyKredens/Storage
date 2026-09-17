using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class GetStockDocumentForReceivingQueryHandler(
    IStockDocumentRepository repository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetStockDocumentForReceivingQuery, StockDocumentDetailsView>
{
    public async ValueTask<StockDocumentDetailsView> Handle(
        GetStockDocumentForReceivingQuery query,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(
            query.Id,
            cancellationToken: cancellationToken)
            ?? throw new ResourceNotFoundException("Przesyłka nie została znaleziona.");

        if (!warehouseContext.CanAccess(document.WarehouseId))
            throw new ForbiddenOperationException("Przesyłka należy do innego magazynu.");
        if (document.Type != StockDocumentType.Receipt)
            throw new CommandValidationException("Podany kod nie jest kodem przesyłki przychodzącej.");

        return StockDocumentMapper.ToDetailsView(document);
    }
}
