using Mediator;

namespace MagazineAPIApplication.Modules.Roles;

public sealed record DeleteRoleCommand(Guid Id) : ICommand<bool>;
