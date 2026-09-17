using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record CompleteStockDocumentCommand(Guid Id) : ICommand<bool>;
