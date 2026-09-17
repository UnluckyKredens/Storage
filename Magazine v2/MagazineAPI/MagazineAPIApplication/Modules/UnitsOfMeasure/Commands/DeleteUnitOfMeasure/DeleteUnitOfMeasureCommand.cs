using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed record DeleteUnitOfMeasureCommand(Guid Id) : ICommand<bool>;
