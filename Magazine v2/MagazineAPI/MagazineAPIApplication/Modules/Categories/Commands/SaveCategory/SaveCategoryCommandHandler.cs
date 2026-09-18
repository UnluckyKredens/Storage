using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.Categories;

public sealed class SaveCategoryCommandHandler(IRepository<Category> repository)
    : ICommandHandler<SaveCategoryCommand, CategoryView>
{
    public async ValueTask<CategoryView> Handle(SaveCategoryCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) throw new CommandValidationException("Pole Name jest wymagane.");


        if (command.Id is null)
        {
            var item = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = command.Name.Trim(),
                Description = command.Description?.Trim()
            };
            await repository.AddAsync(item, cancellationToken);
            return CategoryMapper.ToView(item);
        }

        var existing = await repository.FirstOrDefaultAsync(x => x.CategoryId == command.Id, cancellationToken);
        if (existing is null) throw new ResourceNotFoundException("Rekord nie został znaleziony.");
        existing.Name = command.Name.Trim();
        existing.Description = command.Description?.Trim();
        await repository.UpdateAsync(existing, cancellationToken);
        return CategoryMapper.ToView(existing);
    }
}
