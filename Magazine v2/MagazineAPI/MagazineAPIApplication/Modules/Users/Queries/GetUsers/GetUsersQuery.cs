using Mediator;

namespace MagazineAPIApplication.Modules.Users;

public sealed record GetUsersQuery() : IQuery<IReadOnlyList<UserView>>;
