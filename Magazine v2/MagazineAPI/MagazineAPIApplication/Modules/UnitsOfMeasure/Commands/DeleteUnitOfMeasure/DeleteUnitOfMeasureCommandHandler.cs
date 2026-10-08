using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed class DeleteUnitOfMeasureCommandHandler(IRepository<UnitOfMeasure> repository, IRepository<Product> productRepository)
    : ICommandHandler<DeleteUnitOfMeasureCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteUnitOfMeasureCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.UnitOfMeasureId == command.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if ((await productRepository.FilterByAsync(x => x.UnitOfMeasureId == command.Id, cancellationToken)).Count > 0)
            throw new ResourceConflictException("Jednostka miary jest używana przez produkty.");
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
