using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed class DeleteCategoryCommandHandler(IRepository<Category> repository, IRepository<Product> productRepository)
    : ICommandHandler<DeleteCategoryCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.CategoryId == command.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if ((await productRepository.FilterByAsync(x => x.CategoryId == command.Id, cancellationToken)).Count > 0)
            throw new ResourceConflictException("Kategoria jest używana przez produkty.");
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
