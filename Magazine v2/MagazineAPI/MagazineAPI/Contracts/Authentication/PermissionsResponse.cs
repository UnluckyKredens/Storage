namespace MagazineAPI.Contracts.Authentication;

public sealed record PermissionNameResponse(string Code, string Name);

public sealed record PermissionsResponse(
    IReadOnlyList<string> PermissionCodes,
    IReadOnlyList<PermissionNameResponse> Permissions);
