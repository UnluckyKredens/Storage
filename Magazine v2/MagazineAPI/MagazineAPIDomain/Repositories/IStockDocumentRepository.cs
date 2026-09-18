using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Repositories;

public interface IStockDocumentRepository
{
    Task<(IReadOnlyList<StockDocument> Items, int Total)> GetPageAsync(
        StockDocumentType? type,
        StockDocumentStatus? status,
        Guid? warehouseId,
        string search,
        string sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<StockDocument?> GetByIdAsync(
        Guid id,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(StockDocument document, CancellationToken cancellationToken = default);
    Task ReplaceItemsAsync(
        StockDocument document,
        IReadOnlyCollection<StockDocumentItem> newItems,
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(StockDocument document, CancellationToken cancellationToken = default);
}
