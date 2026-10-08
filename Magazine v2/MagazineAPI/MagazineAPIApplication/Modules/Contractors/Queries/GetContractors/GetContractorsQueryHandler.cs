using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed class GetContractorsQueryHandler(IRepository<Contractor> repository)
    : IQueryHandler<GetContractorsQuery, IReadOnlyList<ContractorView>>
{
    public async ValueTask<IReadOnlyList<ContractorView>> Handle(GetContractorsQuery query, CancellationToken cancellationToken)
    {
        var items = await repository.AllAsync(cancellationToken);
        return items.Select(ContractorMapper.ToView).ToArray();
    }
}
