using Microsoft.AspNetCore.Authorization;

namespace MagazineAPI.Authorization;

public sealed record PermissionRequirement(string PermissionCode) : IAuthorizationRequirement;
