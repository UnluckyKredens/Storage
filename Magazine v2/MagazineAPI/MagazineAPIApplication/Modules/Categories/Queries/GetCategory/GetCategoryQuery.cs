using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed record GetCategoryQuery(Guid Id) : IQuery<CategoryView>;
