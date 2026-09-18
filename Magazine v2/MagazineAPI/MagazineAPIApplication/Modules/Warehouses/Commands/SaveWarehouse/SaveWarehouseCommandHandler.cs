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
                WarehouseId = Guid.NewGuid(),
                Name = command.Name.Trim(),
                Address = command.Address?.Trim(),
                Description = command.Description?.Trim()
            };
            await repository.AddAsync(item, cancellationToken);
            return WarehouseMapper.ToView(item);
        }

        var existing = await repository.FirstOrDefaultAsync(x => x.WarehouseId == command.Id, cancellationToken);
        if (existing is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if (!warehouseContext.IsAdministrator && !warehouseContext.CanAccess(existing.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        existing.Name = command.Name.Trim();
        existing.Address = command.Address?.Trim();
        existing.Description = command.Description?.Trim();
        await repository.UpdateAsync(existing, cancellationToken);
        return WarehouseMapper.ToView(existing);
    }
}
