using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed record GetUserQuery(Guid Id) : IQuery<UserView>;
