using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed record SaveCategoryCommand(
    Guid? Id,
    string Name,
    string? Description) : ICommand<CategoryView>;
