using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

public sealed class SaveUnitOfMeasureCommandHandler(IRepository<UnitOfMeasure> repository)
    : ICommandHandler<SaveUnitOfMeasureCommand, UnitOfMeasureView>
{
    public async ValueTask<UnitOfMeasureView> Handle(SaveUnitOfMeasureCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) throw new CommandValidationException("Pole Name jest wymagane.");
        if (string.IsNullOrWhiteSpace(command.Symbol)) throw new CommandValidationException("Pole Symbol jest wymagane.");


        if (command.Id is null)
        {
            var item = new UnitOfMeasure
            {
                UnitOfMeasureId = Guid.NewGuid(),
                Name = command.Name.Trim(),
                Symbol = command.Symbol.Trim()
            };
            await repository.AddAsync(item, cancellationToken);
            return UnitOfMeasureMapper.ToView(item);
        }

        var existing = await repository.FirstOrDefaultAsync(x => x.UnitOfMeasureId == command.Id, cancellationToken);
        if (existing is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        existing.Name = command.Name.Trim();
        existing.Symbol = command.Symbol.Trim();
        await repository.UpdateAsync(existing, cancellationToken);
        return UnitOfMeasureMapper.ToView(existing);
    }
}
