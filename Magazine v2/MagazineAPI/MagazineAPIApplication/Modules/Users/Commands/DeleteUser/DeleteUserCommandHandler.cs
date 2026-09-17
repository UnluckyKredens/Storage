using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed class DeleteUserCommandHandler(
    IRepository<User> userRepository,
    IRolePermissionRepository rolePermissionRepository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<DeleteUserCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(candidate => candidate.Id == command.UserId, cancellationToken);
        if (user is null)
        {
            throw new ResourceNotFoundException("Użytkownik nie został znaleziony.");
        }

        var actorIsAdministrator = await rolePermissionRepository.UserIsAdministratorAsync(
            command.ActorUserId,
            cancellationToken);

        if (!actorIsAdministrator && user.RoleId == RoleIds.Administrator)
        {
            throw new ForbiddenOperationException(
                "Tylko administrator może usuwać konta administratorów.");
        }

        if (!actorIsAdministrator
            && (user.WarehouseId is null || !warehouseContext.CanAccess(user.WarehouseId.Value)))
        {
            throw new ForbiddenOperationException(
                "Nie można usunąć użytkownika z innego magazynu.");
        }

        if (user.RoleId == RoleIds.Administrator
            && await rolePermissionRepository.CountUsersInRoleAsync(
                RoleIds.Administrator,
                cancellationToken) == 1)
        {
            throw new CommandValidationException(
                "Nie można usunąć ostatniego administratora.");
        }

        await userRepository.DeleteAsync(user, cancellationToken);
        return true;
    }
}
