using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed class GetContractorQueryHandler(IRepository<Contractor> repository)
    : IQueryHandler<GetContractorQuery, ContractorView>
{
    public async ValueTask<ContractorView> Handle(GetContractorQuery query, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.ContractorId == query.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        return ContractorMapper.ToView(item);
    }
}
