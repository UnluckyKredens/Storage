using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed class GetCategoryQueryHandler(IRepository<Category> repository)
    : IQueryHandler<GetCategoryQuery, CategoryView>
{
    public async ValueTask<CategoryView> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.CategoryId == query.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        return CategoryMapper.ToView(item);
    }
}
