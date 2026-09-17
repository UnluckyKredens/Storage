namespace MagazineAPIDomain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
