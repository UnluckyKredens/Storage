using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed record DeleteLocationCommand(Guid Id) : ICommand<bool>;
