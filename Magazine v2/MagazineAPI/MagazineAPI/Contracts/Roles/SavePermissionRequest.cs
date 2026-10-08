using System.ComponentModel.DataAnnotations;

namespace MagazineAPI.Contracts.Roles;

public sealed class SavePermissionRequest
{
    [Required, MaxLength(100)] public string Code { get; init; } = string.Empty;
    [Required, MaxLength(150)] public string Name { get; init; } = string.Empty;
    [MaxLength(500)] public string? Description { get; init; }
}
