using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed record CreateUserCommand(string Login, string FirstName, string LastName,
    string Email, string Password, Guid RoleId, Guid? WarehouseId, Guid ActorUserId) : ICommand<UserView>;
