using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed class GetUnitsOfMeasureQueryHandler(IRepository<UnitOfMeasure> repository)
    : IQueryHandler<GetUnitsOfMeasureQuery, IReadOnlyList<UnitOfMeasureView>>
{
    public async ValueTask<IReadOnlyList<UnitOfMeasureView>> Handle(GetUnitsOfMeasureQuery query, CancellationToken cancellationToken)
    {
        var items = await repository.AllAsync(cancellationToken);
        return items.Select(UnitOfMeasureMapper.ToView).ToArray();
    }
}
