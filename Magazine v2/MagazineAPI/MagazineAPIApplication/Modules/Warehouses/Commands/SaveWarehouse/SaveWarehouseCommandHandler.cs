using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Warehouses;

public sealed class SaveWarehouseCommandHandler(
    IRepository<Warehouse> repository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<SaveWarehouseCommand, WarehouseView>
{
    public async ValueTask<WarehouseView> Handle(SaveWarehouseCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) throw new CommandValidationException("Pole Name jest wymagane.");

        if (command.Id is null)
        {
            if (!warehouseContext.IsAdministrator)
                throw new ForbiddenOperationException("Tylko administrator może utworzyć magazyn.");
            var item = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = command.Name.Trim(),
                Address = command.Address?.Trim(),
                Description = command.Description?.Trim(),
                IsHub = command.IsHub == true
            };
            if (item.IsHub) await ClearOtherHubsAsync(item.Id, cancellationToken);
            await repository.AddAsync(item, cancellationToken);
            return WarehouseMapper.ToView(item);
        }

        var existing = await repository.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (existing is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if (!warehouseContext.IsAdministrator && !warehouseContext.CanAccess(existing.Id))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        existing.Name = command.Name.Trim();
        existing.Address = command.Address?.Trim();
        existing.Description = command.Description?.Trim();
        if (command.IsHub is { } isHub && isHub != existing.IsHub)
        {
            if (!warehouseContext.IsAdministrator)
                throw new ForbiddenOperationException("Tylko administrator może wskazać magazyn przeładunkowy.");
            existing.IsHub = isHub;
            if (isHub) await ClearOtherHubsAsync(existing.Id, cancellationToken);
        }
        await repository.UpdateAsync(existing, cancellationToken);
        return WarehouseMapper.ToView(existing);
    }

    private async Task ClearOtherHubsAsync(Guid keepId, CancellationToken cancellationToken)
    {
        foreach (var other in await repository.FilterByAsync(x => x.IsHub && x.Id != keepId, cancellationToken))
        {
            other.IsHub = false;
            await repository.UpdateAsync(other, cancellationToken);
        }
    }
}
