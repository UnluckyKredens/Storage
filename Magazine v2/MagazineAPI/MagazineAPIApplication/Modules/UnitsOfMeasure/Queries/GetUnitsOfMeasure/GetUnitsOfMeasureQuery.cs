using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed record GetUnitsOfMeasureQuery() : IQuery<IReadOnlyList<UnitOfMeasureView>>;
