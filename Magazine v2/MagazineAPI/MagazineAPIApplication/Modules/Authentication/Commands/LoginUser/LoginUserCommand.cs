using MagazineAPIApplication.Modules.Authentication;
using Mediator;

namespace MagazineAPIApplication.Modules.Authentication;

public sealed record LoginUserCommand(
    string Login,
    string Password) : ICommand<AuthenticationResult>;
