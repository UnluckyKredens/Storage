using System.ComponentModel.DataAnnotations;

namespace MagazineAPI.Contracts.Roles;

public sealed class UpdateRolePermissionsRequest
{
    [Required]
    public IReadOnlyCollection<string> PermissionCodes { get; init; } = [];
}
