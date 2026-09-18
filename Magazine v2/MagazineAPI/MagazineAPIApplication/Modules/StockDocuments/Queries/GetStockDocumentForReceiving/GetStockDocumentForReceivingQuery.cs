using MagazineAPIApplication.Modules.StockDocuments;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record GetStockDocumentForReceivingQuery(Guid Id)
    : IQuery<StockDocumentDetailsView>;
