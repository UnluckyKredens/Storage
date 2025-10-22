using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed record GetUnitOfMeasureQuery(Guid Id) : IQuery<UnitOfMeasureView>;
