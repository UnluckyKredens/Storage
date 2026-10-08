namespace MagazineAPIApplication.Modules.Roles;

public sealed record PermissionResult(
    string Code,
    string Name,
    string? Description);
