using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed class DeleteInventoryCommandHandler(
    IRepository<Inventory> repository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<DeleteInventoryCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteInventoryCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.InventoryId == command.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Stan magazynowy nie został znaleziony.");
        var location = await locationRepository.FirstOrDefaultAsync(
            x => x.LocationId == item.LocationId, cancellationToken)
            ?? throw new ResourceNotFoundException("Lokalizacja nie została znaleziona.");
        if (!warehouseContext.CanAccess(location.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
