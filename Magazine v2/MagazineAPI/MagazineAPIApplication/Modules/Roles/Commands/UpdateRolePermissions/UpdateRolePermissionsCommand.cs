using MagazineAPIApplication.Modules.Roles;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyCollection<string> PermissionCodes) : ICommand<RolePermissionsResult>;
