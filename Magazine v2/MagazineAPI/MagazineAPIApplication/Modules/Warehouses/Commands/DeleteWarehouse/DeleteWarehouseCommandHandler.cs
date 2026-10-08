using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed class DeleteWarehouseCommandHandler(IRepository<Warehouse> repository,
    IRepository<Location> dependentRepository, IRepository<User> userRepository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<DeleteWarehouseCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteWarehouseCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.WarehouseId == command.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if (!warehouseContext.IsAdministrator && !warehouseContext.CanAccess(item.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        if ((await dependentRepository.FilterByAsync(x => x.WarehouseId == command.Id, cancellationToken)).Count > 0)
            throw new ResourceConflictException("Rekord jest używany przez inne dane.");
        if ((await userRepository.FilterByAsync(x => x.WarehouseId == command.Id, cancellationToken)).Count > 0)
            throw new ResourceConflictException("Nie można usunąć magazynu przypisanego do użytkowników.");
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
