using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record DeleteStockDocumentCommand(Guid Id) : ICommand<bool>;
