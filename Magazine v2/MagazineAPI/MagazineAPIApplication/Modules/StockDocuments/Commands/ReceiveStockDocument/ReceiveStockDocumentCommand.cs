using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed record ReceiveStockDocumentCommand(Guid Id) : ICommand<bool>;
