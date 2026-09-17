using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed record GetCategoriesQuery() : IQuery<IReadOnlyList<CategoryView>>;
