using System.Security.Claims;
using MagazineAPIDomain.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace MagazineAPI.Authorization;

public sealed class PermissionAuthorizationHandler(IRolePermissionRepository rolePermissionRepository)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        if (await rolePermissionRepository.UserHasPermissionAsync(
                userId,
                requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }
    }
}
