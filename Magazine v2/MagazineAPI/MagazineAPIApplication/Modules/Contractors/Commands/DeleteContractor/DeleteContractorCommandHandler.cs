using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed class DeleteContractorCommandHandler(IRepository<Contractor> repository)
    : ICommandHandler<DeleteContractorCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteContractorCommand command, CancellationToken cancellationToken)
    {
        var item = await repository.FirstOrDefaultAsync(x => x.ContractorId == command.Id, cancellationToken);
        if (item is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        await repository.DeleteAsync(item, cancellationToken);
        return true;
    }
}
