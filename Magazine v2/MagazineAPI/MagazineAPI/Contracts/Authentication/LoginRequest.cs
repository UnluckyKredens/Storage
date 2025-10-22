using System.ComponentModel.DataAnnotations;

namespace MagazineAPI.Contracts.Authentication;

public sealed class LoginRequest
{
    [Required]
    [MaxLength(255)]
    public string Login { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}
