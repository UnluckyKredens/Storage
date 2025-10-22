namespace MagazineAPI.Contracts.Roles;

public sealed record RoleResponse(
    Guid Id,
    string Name,
    bool HasAllPermissions,
    IReadOnlyList<string> PermissionCodes);
