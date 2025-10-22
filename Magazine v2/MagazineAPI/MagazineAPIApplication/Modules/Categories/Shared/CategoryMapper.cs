using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Modules.Categories;

internal static class CategoryMapper
{
    public static CategoryView ToView(Category item) => new(
            item.Id,
            item.Name,
            item.Description);
}
