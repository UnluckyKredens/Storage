using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Products;

public sealed class SaveProductCommandHandler(
    IRepository<Product> productRepository,
    IRepository<Category> categoryRepository,
    IRepository<UnitOfMeasure> unitRepository) : ICommandHandler<SaveProductCommand, Guid>
{
    public async ValueTask<Guid> Handle(SaveProductCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Sku))
            throw new CommandValidationException("Nazwa i SKU produktu są wymagane.");
        if (command.PurchasePrice < 0 || command.SalePrice < 0)
            throw new CommandValidationException("Ceny nie mogą być ujemne.");
        if (await categoryRepository.FirstOrDefaultAsync(x => x.CategoryId == command.CategoryId, cancellationToken) is null)
            throw new CommandValidationException("Kategoria nie istnieje.");
        if (await unitRepository.FirstOrDefaultAsync(x => x.UnitOfMeasureId == command.UnitOfMeasureId, cancellationToken) is null)
            throw new CommandValidationException("Jednostka miary nie istnieje.");

        var sku = command.Sku.Trim();
        var skuOwner = await productRepository.FirstOrDefaultAsync(x => x.Sku == sku, cancellationToken);
        if (skuOwner is not null && skuOwner.ProductId != command.ProductId)
            throw new ResourceConflictException("Produkt z takim SKU już istnieje.");

        Product product;
        if (command.ProductId is null)
        {
            product = new Product { ProductId = Guid.NewGuid() };
            Assign(product, command, sku);
            await productRepository.AddAsync(product, cancellationToken);
        }
        else
        {
            product = await productRepository.FirstOrDefaultAsync(x => x.ProductId == command.ProductId, cancellationToken)
                ?? throw new ResourceNotFoundException("Produkt nie został znaleziony.");
            Assign(product, command, sku);
            await productRepository.UpdateAsync(product, cancellationToken);
        }

        return product.ProductId;
    }

    private static void Assign(Product product, SaveProductCommand command, string sku)
    {
        product.Name = command.Name.Trim();
        product.Sku = sku;
        product.Barcode = command.Barcode?.Trim();
        product.Description = command.Description?.Trim();
        product.CategoryId = command.CategoryId;
        product.UnitOfMeasureId = command.UnitOfMeasureId;
        product.PurchasePrice = command.PurchasePrice;
        product.SalePrice = command.SalePrice;
        product.IsActive = command.IsActive;
    }
}
