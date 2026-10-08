using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed class GetUserQueryHandler(IRepository<User> userRepository, IRepository<Role> roleRepository,
    IRepository<Warehouse> warehouseRepository, IWarehouseContext warehouseContext)
    : IQueryHandler<GetUserQuery, UserView>
{
    public async ValueTask<UserView> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Użytkownik nie został znaleziony.");
        if (!warehouseContext.IsAdministrator
            && query.Id != warehouseContext.UserId
            && (user.WarehouseId is null || !warehouseContext.CanAccess(user.WarehouseId.Value)))
            throw new ForbiddenOperationException("Brak dostępu do użytkownika z innego magazynu.");
        var role = await roleRepository.FirstOrDefaultAsync(x => x.Id == user.RoleId, cancellationToken);
        var warehouse = user.WarehouseId is null ? null : await warehouseRepository.FirstOrDefaultAsync(
            x => x.WarehouseId == user.WarehouseId, cancellationToken);
        return new UserView(user.Id, user.Login, user.FirstName, user.LastName,
            user.Email, user.RoleId, role?.Name ?? string.Empty, user.WarehouseId, warehouse?.Name);
    }
}
