using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed class GetUsersQueryHandler(IRepository<User> userRepository, IRepository<Role> roleRepository,
    IRepository<Warehouse> warehouseRepository, IWarehouseContext warehouseContext)
    : IQueryHandler<GetUsersQuery, IReadOnlyList<UserView>>
{
    public async ValueTask<IReadOnlyList<UserView>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var users = warehouseContext.IsAdministrator
            ? await userRepository.AllAsync(cancellationToken)
            : await userRepository.FilterByAsync(
                user => user.WarehouseId == warehouseContext.WarehouseId,
                cancellationToken);
        var roles = (await roleRepository.AllAsync(cancellationToken)).ToDictionary(x => x.Id);
        var warehouses = (await warehouseRepository.AllAsync(cancellationToken)).ToDictionary(x => x.WarehouseId);
        return users.Select(x => new UserView(x.Id, x.Login, x.FirstName, x.LastName,
            x.Email, x.RoleId, roles.GetValueOrDefault(x.RoleId)?.Name ?? string.Empty,
            x.WarehouseId, x.WarehouseId is null ? null : warehouses.GetValueOrDefault(x.WarehouseId.Value)?.Name))
            .OrderBy(x => x.Login).ToArray();
    }
}
