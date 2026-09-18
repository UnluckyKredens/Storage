using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed record SaveUnitOfMeasureCommand(
    Guid? Id,
    string Name,
    string Symbol) : ICommand<UnitOfMeasureView>;
