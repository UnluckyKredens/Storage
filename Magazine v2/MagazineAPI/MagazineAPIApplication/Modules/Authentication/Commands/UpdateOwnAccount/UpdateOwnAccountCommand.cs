using MagazineAPIApplication.Modules.Users;
using Mediator;

namespace MagazineAPIApplication.Modules.Authentication;

public sealed record UpdateOwnAccountCommand(
    Guid UserId,
    string Login,
    string FirstName,
    string LastName,
    string Email,
    string? CurrentPassword,
    string? NewPassword) : ICommand<UpdatedAccountResult>;

public sealed record UpdatedAccountResult(string Token, UserView User);
