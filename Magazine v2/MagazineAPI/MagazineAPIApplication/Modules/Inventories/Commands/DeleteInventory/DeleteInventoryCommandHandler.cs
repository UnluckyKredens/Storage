using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using MagazineAPIApplication.Common.Interfaces;

namespace MagazineAPIApplication.Modules.Inventories;

public sealed class DeleteInventoryCommandHandler(
    IRepository<Inventory> repository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext,
    IAuditLogWriter auditLogWriter)
    : ICommandHandler<DeleteInventoryCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteInventoryCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Stan magazynowy nie został znaleziony.");
        var location = await locationRepository.FirstOrDefaultAsync(
            x => x.Id == item.LocationId, cancellationToken)
            ?? throw new ResourceNotFoundException("Lokalizacja nie została znaleziona.");
        if (!warehouseContext.CanAccess(location.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");
        if (item.Quantity != 0 || item.ReservedQuantity != 0)
            throw new CommandValidationException("Można usunąć wyłącznie pusty stan bez rezerwacji. Najpierw zaksięguj korektę.");
        await auditLogWriter.RecordAsync(new AuditLogRecord("Delete", nameof(Inventory), item.Id.ToString(),
            "Usunięto pusty stan magazynowy."), cancellationToken);
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
