using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed class DeleteLocationCommandHandler(
    IRepository<Location> repository,
    IRepository<Inventory> dependentRepository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<DeleteLocationCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.LocationId == command.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if (!warehouseContext.CanAccess(item.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        if ((await dependentRepository.FilterByAsync(x => x.LocationId == command.Id, cancellationToken)).Count > 0)
            throw new ResourceConflictException("Rekord jest używany przez inne dane.");
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
