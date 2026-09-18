using MagazineAPIApplication.Common.ReadModels;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Products;

public sealed class GetProductsQueryHandler(
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IRepository<UnitOfMeasure> unitOfMeasureRepository)
    : IQueryHandler<GetProductsQuery, PaginationReadModel<ProductReadModel>>
{
    public async ValueTask<PaginationReadModel<ProductReadModel>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.AllAsync(cancellationToken);
        var categories = (await categoryRepository.AllAsync(cancellationToken))
            .ToDictionary(category => category.CategoryId, category => category.Name);
        var units = (await unitOfMeasureRepository.AllAsync(cancellationToken))
            .ToDictionary(unit => unit.UnitOfMeasureId, unit => unit.Name);

        var search = query.Search.Trim();
        var filteredProducts = products
            .Where(product => string.IsNullOrEmpty(search)
                || product.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || product.Sku.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (product.Barcode?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var total = filteredProducts.Count();

        var readModels = filteredProducts.Select(product => new ProductReadModel
        {
            ProductId = product.ProductId,
            CategoryId = product.CategoryId,
            UnitOfMeasureId = product.UnitOfMeasureId,
            Name = product.Name,
            Sku = product.Sku,
            Barcode = product.Barcode,
            Description = product.Description,
            UnitOfMeasure = units.GetValueOrDefault(product.UnitOfMeasureId, string.Empty),
            Category = categories.GetValueOrDefault(product.CategoryId, string.Empty),
            PurchasePrice = product.PurchasePrice,
            SalePrice = product.SalePrice,
            IsActive = product.IsActive
        });

        var descending = string.Equals(query.OrderBy, "desc", StringComparison.OrdinalIgnoreCase);
        readModels = (query.SortBy.ToLowerInvariant(), descending) switch
        {
            ("sku", false) => readModels.OrderBy(product => product.Sku),
            ("sku", true) => readModels.OrderByDescending(product => product.Sku),
            ("category", false) => readModels.OrderBy(product => product.Category),
            ("category", true) => readModels.OrderByDescending(product => product.Category),
            ("unitofmeasure", false) => readModels.OrderBy(product => product.UnitOfMeasure),
            ("unitofmeasure", true) => readModels.OrderByDescending(product => product.UnitOfMeasure),
            ("saleprice", false) => readModels.OrderBy(product => product.SalePrice),
            ("saleprice", true) => readModels.OrderByDescending(product => product.SalePrice),
            ("isactive", false) => readModels.OrderBy(product => product.IsActive),
            ("isactive", true) => readModels.OrderByDescending(product => product.IsActive),
            (_, false) => readModels.OrderBy(product => product.Name),
            _ => readModels.OrderByDescending(product => product.Name)
        };

        var results = readModels
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginationReadModel<ProductReadModel>
        {
            PageSize = pageSize,
            Page = page,
            Total = total,
            List = results
        };
    }
}
