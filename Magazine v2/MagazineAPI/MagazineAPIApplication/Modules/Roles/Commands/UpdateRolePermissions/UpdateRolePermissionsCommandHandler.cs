using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Modules.Roles;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed class UpdateRolePermissionsCommandHandler(
    IRolePermissionRepository rolePermissionRepository)
    : ICommandHandler<UpdateRolePermissionsCommand, RolePermissionsResult>
{
    public async ValueTask<RolePermissionsResult> Handle(
        UpdateRolePermissionsCommand command,
        CancellationToken cancellationToken)
    {
        var role = await rolePermissionRepository.GetRoleWithPermissionsAsync(
            command.RoleId,
            cancellationToken);

        if (role is null)
        {
            throw new ResourceNotFoundException("Rola nie została znaleziona.");
        }

        if (role.Id == RoleIds.Administrator)
        {
            throw new CommandValidationException(
                "Uprawnienia administratora są zawsze pełne i nie podlegają konfiguracji.");
        }

        var requestedCodes = command.PermissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var availablePermissions = await rolePermissionRepository.GetPermissionsAsync(cancellationToken);
        var permissionByCode = availablePermissions.ToDictionary(
            permission => permission.Code,
            StringComparer.OrdinalIgnoreCase);

        var unknownCodes = requestedCodes
            .Where(code => !permissionByCode.ContainsKey(code))
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (unknownCodes.Length > 0)
        {
            throw new CommandValidationException(
                $"Nieznane uprawnienia: {string.Join(", ", unknownCodes)}.");
        }

        var permissionIds = requestedCodes
            .Select(code => permissionByCode[code].Id)
            .ToArray();

        await rolePermissionRepository.ReplaceRolePermissionsAsync(
            role.Id,
            permissionIds,
            cancellationToken);

        return new RolePermissionsResult(
            role.Id,
            role.Name,
            false,
            requestedCodes.Order(StringComparer.Ordinal).ToArray());
    }
}
