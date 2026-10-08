using MagazineAPIApplication.Common.ReadModels;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

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
        var search = query.Search.Trim();
        var filteredProducts = productRepository.Query();
        if (!string.IsNullOrEmpty(search))
        {
            filteredProducts = filteredProducts.Where(product =>
                product.Name.Contains(search) ||
                product.Sku.Contains(search) ||
                (product.Barcode != null && product.Barcode.Contains(search)));
        }

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var total = await filteredProducts.CountAsync(cancellationToken);

        var readModels =
            from product in filteredProducts
            join category in categoryRepository.Query()
                on product.CategoryId equals category.CategoryId into categoryJoin
            from category in categoryJoin.DefaultIfEmpty()
            join unit in unitOfMeasureRepository.Query()
                on product.UnitOfMeasureId equals unit.UnitOfMeasureId into unitJoin
            from unit in unitJoin.DefaultIfEmpty()
            select new ProductReadModel
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                UnitOfMeasureId = product.UnitOfMeasureId,
                Name = product.Name,
                Sku = product.Sku,
                Barcode = product.Barcode,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                UnitOfMeasure = unit == null ? string.Empty : unit.Name,
                Category = category == null ? string.Empty : category.Name,
                PurchasePrice = product.PurchasePrice,
                SalePrice = product.SalePrice,
                MinimumQuantity = product.MinimumQuantity,
                OptimumQuantity = product.OptimumQuantity,
                IsActive = product.IsActive
            };

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
            .ToListAsync(cancellationToken);

        return new PaginationReadModel<ProductReadModel>
        {
            PageSize = pageSize,
            Page = page,
            Total = total,
            List = await results
        };
    }
}
