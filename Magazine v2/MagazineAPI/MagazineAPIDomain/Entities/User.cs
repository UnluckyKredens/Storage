namespace MagazineAPIDomain.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Login { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
}
