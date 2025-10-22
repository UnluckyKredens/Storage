using System.Linq.Expressions;
using MagazineAPIDomain.Repositories;
using Microsoft.EntityFrameworkCore;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Entities.History;
using System.Text.Json;

namespace MagazineAPInfrastructure.Persistence.Repositories;

public sealed class HistoryRepository<TEntity>(HistoryDbContext dbContext, AppDbContext operationalContext) : IRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public IQueryable<TEntity> Query()
    {
        return _dbSet.AsNoTracking();
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        switch (entity)
        {
            case InventoryHistory inventory: inventory.Id = id; break;
            case ShipmentHistory shipment: shipment.Id = id; break;
            default: throw new InvalidOperationException("Nieobsługiwany typ historii.");
        }
        operationalContext.HistoryOutboxMessages.Add(new HistoryOutboxMessage
        {
            Id = id, EntityType = typeof(TEntity).Name,
            Payload = JsonSerializer.Serialize(entity), CreatedOnUtc = DateTime.UtcNow
        });
        await operationalContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity?> FindByAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FilterByAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> AllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
