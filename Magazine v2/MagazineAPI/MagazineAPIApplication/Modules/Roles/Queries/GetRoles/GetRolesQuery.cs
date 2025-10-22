using MagazineAPIApplication.Modules.Roles;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed record GetRolesQuery : IQuery<IReadOnlyList<RolePermissionsResult>>;
