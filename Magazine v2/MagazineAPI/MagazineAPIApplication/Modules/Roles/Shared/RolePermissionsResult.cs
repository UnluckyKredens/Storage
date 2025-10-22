namespace MagazineAPIApplication.Modules.Roles;

public sealed record RolePermissionsResult(
    Guid Id,
    string Name,
    bool HasAllPermissions,
    IReadOnlyList<string> PermissionCodes);
