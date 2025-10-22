using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed class GetPermissionCatalogQueryHandler(IRepository<Permission> repository)
    : IQueryHandler<GetPermissionCatalogQuery, IReadOnlyList<PermissionView>>
{
    public async ValueTask<IReadOnlyList<PermissionView>> Handle(GetPermissionCatalogQuery query, CancellationToken cancellationToken)
    {
        return (await repository.AllAsync(cancellationToken)).Select(PermissionMapper.ToView)
            .OrderBy(x => x.Code).ToArray();
    }
}
