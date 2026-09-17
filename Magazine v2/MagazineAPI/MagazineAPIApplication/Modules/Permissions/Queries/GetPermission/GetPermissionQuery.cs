using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed record GetPermissionQuery(Guid Id) : IQuery<PermissionView>;
