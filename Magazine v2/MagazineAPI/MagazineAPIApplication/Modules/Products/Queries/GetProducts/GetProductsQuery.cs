using MagazineAPIApplication.Common.ReadModels;
using Mediator;

namespace MagazineAPIApplication.Modules.Products;

public class GetProductsQuery : IQuery<PaginationReadModel<ProductReadModel>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string Search { get; set; } = string.Empty;
    public string SortBy { get; set; } = "name";
    public string OrderBy { get; set; } = string.Empty;
}
