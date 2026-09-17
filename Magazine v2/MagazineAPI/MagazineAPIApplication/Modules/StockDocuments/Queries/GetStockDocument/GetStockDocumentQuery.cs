using MagazineAPIApplication.Modules.StockDocuments;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record GetStockDocumentQuery(Guid Id) : IQuery<StockDocumentDetailsView>;
