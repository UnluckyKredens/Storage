using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed record DeletePermissionCommand(Guid Id) : ICommand<bool>;
