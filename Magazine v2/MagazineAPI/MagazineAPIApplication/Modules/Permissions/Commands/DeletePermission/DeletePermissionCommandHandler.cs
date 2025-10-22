using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed class DeletePermissionCommandHandler(IRepository<Permission> repository)
    : ICommandHandler<DeletePermissionCommand, bool>
{
    public async ValueTask<bool> Handle(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        if (PermissionMapper.IsSystemPermission(command.Id))
            throw new ForbiddenOperationException("Nie można usunąć uprawnienia systemowego.");
        var item = await repository.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Uprawnienie nie zostało znalezione.");
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
