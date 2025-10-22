using System.ComponentModel.DataAnnotations;

namespace MagazineAPI.Contracts.Users;

public sealed class CreateUserRequest
{
    [Required, MaxLength(100)] public string Login { get; init; } = string.Empty;
    [Required, MaxLength(100)] public string FirstName { get; init; } = string.Empty;
    [Required, MaxLength(100)] public string LastName { get; init; } = string.Empty;
    [Required, EmailAddress, MaxLength(255)] public string Email { get; init; } = string.Empty;
    [Required, MinLength(8), MaxLength(128)] public string Password { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public Guid? WarehouseId { get; init; }
}
