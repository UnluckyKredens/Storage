using Microsoft.AspNetCore.Authorization;

namespace MagazineAPI.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    internal const string PolicyPrefix = "Permission:";

    public HasPermissionAttribute(string permissionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);
        Policy = $"{PolicyPrefix}{permissionCode}";
    }
}
