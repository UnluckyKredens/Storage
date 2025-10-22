namespace MagazineAPI.Contracts.Authentication;

public sealed record PermissionsResponse(IReadOnlyList<string> PermissionCodes);
