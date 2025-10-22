using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed class DeleteRoleCommandHandler(IRepository<Role> roleRepository, IRolePermissionRepository rolePermissionRepository)
    : ICommandHandler<DeleteRoleCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == RoleIds.Administrator || command.Id == RoleIds.Manager || command.Id == RoleIds.User)
            throw new ForbiddenOperationException("Nie można usunąć roli systemowej.");
        var role = await roleRepository.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Rola nie została znaleziona.");
        if (await rolePermissionRepository.CountUsersInRoleAsync(command.Id, cancellationToken) > 0)
            throw new ResourceConflictException("Rola jest przypisana do użytkowników.");
        await roleRepository.DeleteAsync(role, cancellationToken);
        return true;
    }
}
