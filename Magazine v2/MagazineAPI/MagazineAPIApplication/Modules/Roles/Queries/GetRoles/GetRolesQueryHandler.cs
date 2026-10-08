using MagazineAPIApplication.Modules.Roles;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed class GetRolesQueryHandler(IRolePermissionRepository rolePermissionRepository)
    : IQueryHandler<GetRolesQuery, IReadOnlyList<RolePermissionsResult>>
{
    public async ValueTask<IReadOnlyList<RolePermissionsResult>> Handle(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        var permissions = await rolePermissionRepository.GetPermissionsAsync(cancellationToken);
        var allPermissionCodes = permissions
            .Select(permission => permission.Code)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var roles = await rolePermissionRepository.GetRolesWithPermissionsAsync(cancellationToken);

        return roles
            .Select(role => new RolePermissionsResult(
                role.Id,
                role.Name,
                role.Id == RoleIds.Administrator,
                role.Id == RoleIds.Administrator
                    ? allPermissionCodes
                    : role.RolePermissions
                        .Select(rolePermission => rolePermission.Permission.Code)
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
            .ToArray();
    }
}
