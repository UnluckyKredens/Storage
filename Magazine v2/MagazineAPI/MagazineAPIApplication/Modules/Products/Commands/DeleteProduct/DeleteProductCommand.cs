using Mediator;

namespace MagazineAPIApplication.Modules.Products;

public sealed record DeleteProductCommand(Guid Id) : ICommand<bool>;
