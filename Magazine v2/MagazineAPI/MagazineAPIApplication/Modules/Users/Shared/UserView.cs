namespace MagazineAPIApplication.Modules.Users;

public sealed record UserView(Guid Id, string Login, string FirstName, string LastName,
    string Email, Guid RoleId, string RoleName, Guid? WarehouseId, string? WarehouseName);
