using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed class SaveLocationCommandHandler(
    IRepository<Location> repository,
    IRepository<Warehouse> warehouseRepository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<SaveLocationCommand, LocationView>
{
    public async ValueTask<LocationView> Handle(SaveLocationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.LocationCode)) throw new CommandValidationException("Pole LocationCode jest wymagane.");
        if (command.WarehouseId == Guid.Empty) throw new CommandValidationException("Pole WarehouseId jest wymagane.");
        var warehouse = await warehouseRepository.FirstOrDefaultAsync(x => x.WarehouseId == command.WarehouseId, cancellationToken)
            ?? throw new CommandValidationException("Magazyn nie istnieje.");
        if (!warehouseContext.CanAccess(warehouse.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        if (command.Id is null)
        {
            var item = new Location
            {
                LocationId = Guid.NewGuid(),
                WarehouseId = command.WarehouseId,
                LocationCode = command.LocationCode.Trim(),
                Description = command.Description?.Trim()
            };
            await repository.AddAsync(item, cancellationToken);
            return LocationMapper.ToView(item, warehouse.Name);
        }

        var existing = await repository.FirstOrDefaultAsync(x => x.LocationId == command.Id, cancellationToken);
        if (existing is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        if (!warehouseContext.CanAccess(existing.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        existing.WarehouseId = command.WarehouseId;
        existing.LocationCode = command.LocationCode.Trim();
        existing.Description = command.Description?.Trim();
        await repository.UpdateAsync(existing, cancellationToken);
        return LocationMapper.ToView(existing, warehouse.Name);
    }
}
