namespace MagazineAPIApplication.Modules.Categories;

public sealed record CategoryView(
    Guid Id,
    string Name,
    string? Description);
