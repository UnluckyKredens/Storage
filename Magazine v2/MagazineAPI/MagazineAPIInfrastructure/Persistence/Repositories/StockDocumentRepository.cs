using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPInfrastructure.Persistence.Repositories;

public sealed class StockDocumentRepository(AppDbContext dbContext) : IStockDocumentRepository
{
    public async Task<(IReadOnlyList<StockDocument> Items, int Total)> GetPageAsync(
        StockDocumentType? type,
        StockDocumentStatus? status,
        Guid? warehouseId,
        string search,
        string sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.StockDocuments
            .AsNoTracking()
            .Include(document => document.Warehouse)
            .Include(document => document.Contractor)
            .Include(document => document.DestinationWarehouse)
            .Include(document => document.ApprovedByUser)
            .Include(document => document.ReceivedByUser)
            .Include(document => document.Items)
            .AsQueryable();

        if (type is not null)
            query = query.Where(document => document.Type == type);
        if (status is not null)
            query = query.Where(document => document.Status == status);

        if (warehouseId is not null)
            query = query.Where(document => document.WarehouseId == warehouseId);

        var phrase = search.Trim();
        if (phrase.Length > 0)
        {
            query = query.Where(document =>
                document.Number.Contains(phrase)
                || (document.Contractor != null && document.Contractor.Name.Contains(phrase))
                || (document.DestinationWarehouse != null
                    && document.DestinationWarehouse.Name.Contains(phrase)));
        }

        var total = await query.CountAsync(cancellationToken);
        query = (sortBy.ToLowerInvariant(), descending) switch
        {
            ("number", false) => query.OrderBy(document => document.Number),
            ("number", true) => query.OrderByDescending(document => document.Number),
            ("type", false) => query.OrderBy(document => document.Type),
            ("type", true) => query.OrderByDescending(document => document.Type),
            ("status", false) => query.OrderBy(document => document.Status),
            ("status", true) => query.OrderByDescending(document => document.Status),
            ("warehouse", false) => query.OrderBy(document => document.Warehouse.Name),
            ("warehouse", true) => query.OrderByDescending(document => document.Warehouse.Name),
            ("destination", false) => query.OrderBy(document => document.Contractor != null
                ? document.Contractor.Name
                : document.DestinationWarehouse != null ? document.DestinationWarehouse.Name : ""),
            ("destination", true) => query.OrderByDescending(document => document.Contractor != null
                ? document.Contractor.Name
                : document.DestinationWarehouse != null ? document.DestinationWarehouse.Name : ""),
            ("approvedby", false) => query.OrderBy(document => document.ApprovedByUser != null
                ? document.ApprovedByUser.FirstName + " " + document.ApprovedByUser.LastName
                : ""),
            ("approvedby", true) => query.OrderByDescending(document => document.ApprovedByUser != null
                ? document.ApprovedByUser.FirstName + " " + document.ApprovedByUser.LastName
                : ""),
            ("items", false) => query.OrderBy(document => document.Items.Count),
            ("items", true) => query.OrderByDescending(document => document.Items.Count),
            ("completed", false) => query.OrderBy(document => document.CompletedAtUtc),
            ("completed", true) => query.OrderByDescending(document => document.CompletedAtUtc),
            ("created", false) => query.OrderBy(document => document.CreatedAtUtc),
            _ => query.OrderByDescending(document => document.CreatedAtUtc)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<StockDocument?> GetByIdAsync(
        Guid id,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.StockDocuments
            .Include(document => document.Warehouse)
            .Include(document => document.Contractor)
            .Include(document => document.DestinationWarehouse)
            .Include(document => document.CreatedByUser)
            .Include(document => document.ApprovedByUser)
            .Include(document => document.ReceivedByUser)
            .Include(document => document.Items)
                .ThenInclude(item => item.Product)
            .Include(document => document.Items)
                .ThenInclude(item => item.Location)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return query.SingleOrDefaultAsync(document => document.Id == id, cancellationToken);
    }

    public async Task AddAsync(StockDocument document, CancellationToken cancellationToken = default)
    {
        await dbContext.StockDocuments.AddAsync(document, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceItemsAsync(
        StockDocument document,
        IReadOnlyCollection<StockDocumentItem> newItems,
        CancellationToken cancellationToken = default)
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            dbContext.StockDocumentItems.RemoveRange(document.Items);
            await dbContext.SaveChangesAsync(cancellationToken);

            document.Items.Clear();
            foreach (var item in newItems)
            {
                document.Items.Add(item);
            }
            await dbContext.StockDocumentItems.AddRangeAsync(newItems, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(StockDocument document, CancellationToken cancellationToken = default)
    {
        dbContext.StockDocuments.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
