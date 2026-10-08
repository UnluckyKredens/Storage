using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Products;

public sealed class DeleteProductCommandHandler(
    IRepository<Product> productRepository,
    IRepository<Inventory> inventoryRepository) : ICommandHandler<DeleteProductCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await productRepository.FirstOrDefaultAsync(x => x.ProductId == command.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Produkt nie został znaleziony.");
        if ((await inventoryRepository.FilterByAsync(x => x.ProductId == command.Id, cancellationToken)).Count > 0)
            throw new ResourceConflictException("Nie można usunąć produktu, który ma stan magazynowy.");

        await productRepository.DeleteAsync(product, cancellationToken);
        return true;
    }
}
