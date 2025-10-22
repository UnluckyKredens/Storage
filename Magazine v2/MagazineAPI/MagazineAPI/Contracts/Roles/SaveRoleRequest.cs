using System.ComponentModel.DataAnnotations;

namespace MagazineAPI.Contracts.Roles;

public sealed class SaveRoleRequest
{
    [Required, MaxLength(50)]
    public string Name { get; init; } = string.Empty;
}
