using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand<bool>;
