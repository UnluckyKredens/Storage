using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Modules.Roles;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed class GetRoleQueryHandler(IRolePermissionRepository rolePermissionRepository)
    : IQueryHandler<GetRoleQuery, RolePermissionsResult>
{
    public async ValueTask<RolePermissionsResult> Handle(GetRoleQuery query, CancellationToken cancellationToken)
    {
        var role = await rolePermissionRepository.GetRoleWithPermissionsAsync(query.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Rola nie została znaleziona.");
        var allPermissions = role.Id == RoleIds.Administrator
            ? (await rolePermissionRepository.GetPermissionsAsync(cancellationToken)).Select(x => x.Code)
            : role.RolePermissions.Select(x => x.Permission.Code);
        return new RolePermissionsResult(role.Id, role.Name, role.Id == RoleIds.Administrator,
            allPermissions.Order(StringComparer.Ordinal).ToArray());
    }
}
