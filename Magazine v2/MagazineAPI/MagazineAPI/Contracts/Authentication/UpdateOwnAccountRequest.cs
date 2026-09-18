using System.ComponentModel.DataAnnotations;

namespace MagazineAPI.Contracts.Authentication;

public sealed class UpdateOwnAccountRequest
{
    [Required]
    [MaxLength(100)]
    public string Login { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    public string? CurrentPassword { get; init; }

    [MinLength(8)]
    [MaxLength(128)]
    public string? NewPassword { get; init; }
}
