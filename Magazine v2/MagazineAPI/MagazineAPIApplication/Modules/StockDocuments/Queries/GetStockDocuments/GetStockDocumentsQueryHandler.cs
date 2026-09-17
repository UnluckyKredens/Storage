using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.ReadModels;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class GetStockDocumentsQueryHandler(
    IStockDocumentRepository repository,
    IWarehouseContext warehouseContext)
    : IQueryHandler<GetStockDocumentsQuery, PaginationReadModel<StockDocumentListView>>
{
    public async ValueTask<PaginationReadModel<StockDocumentListView>> Handle(
        GetStockDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Type is not null && !Enum.IsDefined(query.Type.Value))
            throw new CommandValidationException("Nieprawidłowy typ dokumentu.");
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await repository.GetPageAsync(
            query.Type,
            query.Status,
            warehouseContext.WarehouseId,
            query.Search,
            query.SortBy,
            string.Equals(query.Order, "desc", StringComparison.OrdinalIgnoreCase),
            page,
            pageSize,
            cancellationToken);

        return new PaginationReadModel<StockDocumentListView>
        {
            List = result.Items.Select(StockDocumentMapper.ToListView).ToList(),
            Total = result.Total,
            Page = page,
            PageSize = pageSize
        };
    }
}
