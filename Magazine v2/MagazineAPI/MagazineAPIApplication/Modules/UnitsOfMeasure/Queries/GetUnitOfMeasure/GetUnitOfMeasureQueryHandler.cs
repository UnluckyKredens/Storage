using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed class GetUnitOfMeasureQueryHandler(IRepository<UnitOfMeasure> repository)
    : IQueryHandler<GetUnitOfMeasureQuery, UnitOfMeasureView>
{
    public async ValueTask<UnitOfMeasureView> Handle(GetUnitOfMeasureQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.UnitOfMeasureId == query.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        return UnitOfMeasureMapper.ToView(item);
    }
}
