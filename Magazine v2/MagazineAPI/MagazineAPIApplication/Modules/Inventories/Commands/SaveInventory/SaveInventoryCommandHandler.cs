using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed class SaveInventoryCommandHandler(
    IRepository<Inventory> inventoryRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext) : ICommandHandler<SaveInventoryCommand, Guid>
{
    public async ValueTask<Guid> Handle(SaveInventoryCommand command, CancellationToken cancellationToken)
    {
        if (command.Quantity < 0 || command.ReservedQuantity < 0 || command.ReservedQuantity > command.Quantity)
            throw new CommandValidationException("Ilość i rezerwacja muszą być nieujemne, a rezerwacja nie może przekraczać ilości.");
        if (await productRepository.FirstOrDefaultAsync(x => x.ProductId == command.ProductId, cancellationToken) is null)
            throw new CommandValidationException("Produkt nie istnieje.");
        var location = await locationRepository.FirstOrDefaultAsync(
            x => x.LocationId == command.LocationId, cancellationToken)
            ?? throw new CommandValidationException("Lokalizacja nie istnieje.");
        if (!warehouseContext.CanAccess(location.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");

        var samePair = await inventoryRepository.FirstOrDefaultAsync(
            x => x.ProductId == command.ProductId && x.LocationId == command.LocationId, cancellationToken);
        if (samePair is not null && samePair.InventoryId != command.Id)
            throw new ResourceConflictException("Stan tego produktu w tej lokalizacji już istnieje.");

        Inventory item;
        if (command.Id is null)
        {
            item = new Inventory { InventoryId = Guid.NewGuid() };
            Assign(item, command);
            await inventoryRepository.AddAsync(item, cancellationToken);
        }
        else
        {
            item = await inventoryRepository.FirstOrDefaultAsync(x => x.InventoryId == command.Id, cancellationToken)
                ?? throw new ResourceNotFoundException("Stan magazynowy nie został znaleziony.");
            var currentLocation = await locationRepository.FirstOrDefaultAsync(
                x => x.LocationId == item.LocationId, cancellationToken);
            if (currentLocation is not null && !warehouseContext.CanAccess(currentLocation.WarehouseId))
                throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
            Assign(item, command);
            await inventoryRepository.UpdateAsync(item, cancellationToken);
        }
        return item.InventoryId;
    }

    private static void Assign(Inventory item, SaveInventoryCommand command)
    {
        item.ProductId = command.ProductId;
        item.LocationId = command.LocationId;
        item.Quantity = command.Quantity;
        item.ReservedQuantity = command.ReservedQuantity;
    }
}
