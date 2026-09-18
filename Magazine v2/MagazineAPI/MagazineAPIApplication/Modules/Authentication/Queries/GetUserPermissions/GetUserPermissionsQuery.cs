using Mediator;

namespace MagazineAPIApplication.Modules.Authentication;

public sealed record GetUserPermissionsQuery(Guid UserId) : IQuery<IReadOnlyList<string>>;
