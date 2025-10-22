using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed class GetCategoriesQueryHandler(IRepository<Category> repository)
    : IQueryHandler<GetCategoriesQuery, IReadOnlyList<CategoryView>>
{
    public async ValueTask<IReadOnlyList<CategoryView>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var items = await repository.AllAsync(cancellationToken);
        return items.Select(CategoryMapper.ToView).ToArray();
    }
}
