using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed record DeleteUserCommand(
    Guid UserId,
    Guid ActorUserId) : ICommand<bool>;
