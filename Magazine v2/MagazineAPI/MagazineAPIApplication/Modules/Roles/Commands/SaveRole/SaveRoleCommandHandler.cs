using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Modules.Roles;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed class SaveRoleCommandHandler(IRepository<Role> roleRepository, IRolePermissionRepository rolePermissionRepository)
    : ICommandHandler<SaveRoleCommand, RolePermissionsResult>
{
    public async ValueTask<RolePermissionsResult> Handle(SaveRoleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new CommandValidationException("Nazwa roli jest wymagana.");
        var name = command.Name.Trim();
        var owner = await roleRepository.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
        if (owner is not null && owner.Id != command.Id)
            throw new ResourceConflictException("Rola o tej nazwie już istnieje.");

        Role role;
        if (command.Id is null)
        {
            role = new Role { Id = Guid.NewGuid(), Name = name };
            await roleRepository.AddAsync(role, cancellationToken);
        }
        else
        {
            if (IsBuiltIn(command.Id.Value))
                throw new ForbiddenOperationException("Nie można zmienić nazwy roli systemowej.");
            role = await roleRepository.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
                ?? throw new ResourceNotFoundException("Rola nie została znaleziona.");
            role.Name = name;
            await roleRepository.UpdateAsync(role, cancellationToken);
        }

        return await new GetRoleQueryHandler(rolePermissionRepository).Handle(new GetRoleQuery(role.Id), cancellationToken);
    }

    private static bool IsBuiltIn(Guid id) => id == RoleIds.Administrator || id == RoleIds.Manager || id == RoleIds.User;
}
