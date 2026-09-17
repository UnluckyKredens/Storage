using MagazineAPIApplication.Common.ReadModels;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Enums;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record GetStockDocumentsQuery(
    StockDocumentType? Type,
    StockDocumentStatus? Status,
    int Page = 1,
    int PageSize = 10,
    string Search = "",
    string SortBy = "created",
    string Order = "desc") : IQuery<PaginationReadModel<StockDocumentListView>>;
