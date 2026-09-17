using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.ReadModels;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Products;

public sealed class GetProductQueryHandler(
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IRepository<UnitOfMeasure> unitRepository) : IQueryHandler<GetProductQuery, ProductReadModel>
{
    public async ValueTask<ProductReadModel> Handle(GetProductQuery query, CancellationToken cancellationToken)
    {
        var product = await productRepository.FirstOrDefaultAsync(x => x.ProductId == query.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Produkt nie został znaleziony.");
        var category = await categoryRepository.FirstOrDefaultAsync(x => x.CategoryId == product.CategoryId, cancellationToken);
        var unit = await unitRepository.FirstOrDefaultAsync(x => x.UnitOfMeasureId == product.UnitOfMeasureId, cancellationToken);

        return new ProductReadModel
        {
            ProductId = product.ProductId,
            CategoryId = product.CategoryId,
            UnitOfMeasureId = product.UnitOfMeasureId,
            Name = product.Name,
            Sku = product.Sku,
            Barcode = product.Barcode,
            Description = product.Description,
            Category = category?.Name ?? string.Empty,
            UnitOfMeasure = unit?.Name,
            PurchasePrice = product.PurchasePrice,
            SalePrice = product.SalePrice,
            IsActive = product.IsActive
        };
    }
}
