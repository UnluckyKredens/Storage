using MagazineAPIApplication.Modules.Roles;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed record GetRoleQuery(Guid Id) : IQuery<RolePermissionsResult>;
