using System.Text.RegularExpressions;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed class SavePermissionCommandHandler(IRepository<Permission> repository)
    : ICommandHandler<SavePermissionCommand, PermissionView>
{
    public async ValueTask<PermissionView> Handle(SavePermissionCommand command, CancellationToken cancellationToken)
    {
        var code = command.Code.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(command.Name) || code.Length > 100
            || !Regex.IsMatch(code, "^[a-z][a-z0-9.]*$") || code == PermissionCodes.RolesManage)
            throw new CommandValidationException("Podaj nazwę i poprawny kod uprawnienia.");
        var owner = await repository.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (owner is not null && owner.Id != command.Id)
            throw new ResourceConflictException("Kod uprawnienia jest już zajęty.");

        Permission item;
        if (command.Id is null)
        {
            item = new Permission { Id = Guid.NewGuid(), Code = code, Name = command.Name.Trim(), Description = command.Description?.Trim() };
            await repository.AddAsync(item, cancellationToken);
        }
        else
        {
            if (PermissionMapper.IsSystemPermission(command.Id.Value))
                throw new ForbiddenOperationException("Nie można zmienić uprawnienia systemowego.");
            item = await repository.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
                ?? throw new ResourceNotFoundException("Uprawnienie nie zostało znalezione.");
            item.Code = code;
            item.Name = command.Name.Trim();
            item.Description = command.Description?.Trim();
            await repository.UpdateAsync(item, cancellationToken);
        }
        return PermissionMapper.ToView(item);
    }
}
