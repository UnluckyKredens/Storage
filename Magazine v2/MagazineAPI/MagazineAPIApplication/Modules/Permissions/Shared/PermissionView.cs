namespace MagazineAPIApplication.Modules.Permissions;

public sealed record PermissionView(Guid Id, string Code, string Name, string? Description);
