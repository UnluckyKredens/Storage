using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed record SavePermissionCommand(Guid? Id, string Code, string Name, string? Description) : ICommand<PermissionView>;
