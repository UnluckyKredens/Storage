using MagazineAPIApplication.Modules.Roles;
using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed record SaveRoleCommand(Guid? Id, string Name) : ICommand<RolePermissionsResult>;
