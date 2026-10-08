using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed class SaveContractorCommandHandler(IRepository<Contractor> repository)
    : ICommandHandler<SaveContractorCommand, ContractorView>
{
    public async ValueTask<ContractorView> Handle(SaveContractorCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) throw new CommandValidationException("Pole Name jest wymagane.");
        if (string.IsNullOrWhiteSpace(command.TaxNumber)) throw new CommandValidationException("Pole TaxNumber jest wymagane.");

        if (!Enum.IsDefined(command.Type))
            throw new CommandValidationException("Nieprawidłowy typ kontrahenta.");
        if (command.Id is null)
        {
            var item = new Contractor
            {
                ContractorId = Guid.NewGuid(),
                Name = command.Name.Trim(),
                TaxNumber = command.TaxNumber.Trim(),
                Type = command.Type,
                Email = command.Email?.Trim(),
                Phone = command.Phone?.Trim(),
                Address = command.Address?.Trim()
            };
            await repository.AddAsync(item, cancellationToken);
            return ContractorMapper.ToView(item);
        }

        var existing = await repository.FirstOrDefaultAsync(x => x.ContractorId == command.Id, cancellationToken);
        if (existing is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        existing.Name = command.Name.Trim();
        existing.TaxNumber = command.TaxNumber.Trim();
        existing.Type = command.Type;
        existing.Email = command.Email?.Trim();
        existing.Phone = command.Phone?.Trim();
        existing.Address = command.Address?.Trim();
        await repository.UpdateAsync(existing, cancellationToken);
        return ContractorMapper.ToView(existing);
    }
}
