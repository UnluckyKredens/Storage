using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Authentication;

public sealed class GetUserPermissionsQueryHandler(
    IRolePermissionRepository rolePermissionRepository)
    : IQueryHandler<GetUserPermissionsQuery, IReadOnlyList<string>>
{
    public async ValueTask<IReadOnlyList<string>> Handle(
        GetUserPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        return await rolePermissionRepository.GetUserPermissionCodesAsync(
            query.UserId,
            cancellationToken);
    }
}
