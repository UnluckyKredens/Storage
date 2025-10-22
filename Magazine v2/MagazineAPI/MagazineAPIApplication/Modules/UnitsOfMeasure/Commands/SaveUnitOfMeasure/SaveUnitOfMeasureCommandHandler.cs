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


        var name = command.Name.Trim();
        var symbol = command.Symbol.Trim();
        var duplicates = await repository.FilterByAsync(
            x => x.Id != command.Id && (x.Name.ToLower() == name.ToLower() || x.Symbol.ToLower() == symbol.ToLower()),
            cancellationToken);
        if (duplicates.Count > 0)
            throw new ResourceConflictException("Jednostka o takiej nazwie lub symbolu już istnieje.");

        if (command.Id is null)
        {
            var item = new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                Name = command.Name.Trim(),
                Symbol = command.Symbol.Trim()
            };
            await repository.AddAsync(item, cancellationToken);
            return UnitOfMeasureMapper.ToView(item);
        }

        var existing = await repository.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (existing is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        existing.Name = command.Name.Trim();
        existing.Symbol = command.Symbol.Trim();
        await repository.UpdateAsync(existing, cancellationToken);
        return UnitOfMeasureMapper.ToView(existing);
    }
}
