using MagazineAPIApplication.Modules.Roles;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed class GetPermissionsQueryHandler(IRolePermissionRepository rolePermissionRepository)
    : IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionResult>>
{
    public async ValueTask<IReadOnlyList<PermissionResult>> Handle(
        GetPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var permissions = await rolePermissionRepository.GetPermissionsAsync(cancellationToken);

        return permissions
            .Select(permission => new PermissionResult(
                permission.Code,
                permission.Name,
                permission.Description))
            .ToArray();
    }
}
