namespace MagazineAPI.Contracts.Roles;

public sealed record PermissionResponse(
    string Code,
    string Name,
    string? Description);
