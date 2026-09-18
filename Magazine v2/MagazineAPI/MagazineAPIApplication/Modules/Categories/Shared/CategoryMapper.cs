using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Modules.Categories;

internal static class CategoryMapper
{
    public static CategoryView ToView(Category item) => new(
            item.CategoryId,
            item.Name,
            item.Description);
}
