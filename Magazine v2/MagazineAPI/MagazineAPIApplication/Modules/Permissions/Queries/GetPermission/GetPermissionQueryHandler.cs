using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed class GetPermissionQueryHandler(IRepository<Permission> repository)
    : IQueryHandler<GetPermissionQuery, PermissionView>
{
    public async ValueTask<PermissionView> Handle(GetPermissionQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Uprawnienie nie zostało znalezione.");
        return PermissionMapper.ToView(item);
    }
}
