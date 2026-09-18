using System.Linq.Expressions;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Xunit;

namespace MagazineAPIApplication.Tests;

public sealed class CompleteStockDocumentCommandHandlerTests
{
    [Fact]
    public async Task ReceiptIncreasesInventoryAndCompletesDocument()
    {
        var data = TestData(StockDocumentType.Receipt, inventoryQuantity: 5, reservedQuantity: 1, itemQuantity: 3);
        data.Document.Status = StockDocumentStatus.Received;
        var handler = CreateHandler(data);

        await handler.Handle(new CompleteStockDocumentCommand(data.Document.Id), CancellationToken.None);

        Assert.Equal(8, Assert.Single(data.Inventories.Items).Quantity);
        Assert.Equal(StockDocumentStatus.Completed, data.Document.Status);
        Assert.NotNull(data.Document.CompletedAtUtc);
        Assert.NotNull(data.Document.ApprovedByUserId);
    }

    [Fact]
    public async Task EmployeeCanMarkReceiptAsReceivedWithoutChangingInventory()
    {
        var data = TestData(StockDocumentType.Receipt, inventoryQuantity: 5, reservedQuantity: 1, itemQuantity: 3);
        var handler = new ReceiveStockDocumentCommandHandler(
            new StockDocumentRepository(data.Document),
            new WarehouseContext(data.Document.WarehouseId));

        await handler.Handle(new ReceiveStockDocumentCommand(data.Document.Id), CancellationToken.None);

        Assert.Equal(StockDocumentStatus.Received, data.Document.Status);
        Assert.NotNull(data.Document.ReceivedAtUtc);
        Assert.NotNull(data.Document.ReceivedByUserId);
        Assert.Equal(5, Assert.Single(data.Inventories.Items).Quantity);
    }

    [Fact]
    public async Task ReceiptMustBeReceivedBeforeManagerApproval()
    {
        var data = TestData(StockDocumentType.Receipt, inventoryQuantity: 5, reservedQuantity: 1, itemQuantity: 3);
        var handler = CreateHandler(data);

        await Assert.ThrowsAsync<ResourceConflictException>(async () =>
            await handler.Handle(
                new CompleteStockDocumentCommand(data.Document.Id),
                CancellationToken.None));

        Assert.Equal(StockDocumentStatus.Draft, data.Document.Status);
        Assert.Equal(5, Assert.Single(data.Inventories.Items).Quantity);
    }

    [Fact]
    public async Task ShipmentRequiresAvailableInventory()
    {
        var data = TestData(StockDocumentType.Shipment, inventoryQuantity: 5, reservedQuantity: 4, itemQuantity: 2);
        var handler = CreateHandler(data);

        await Assert.ThrowsAsync<ResourceConflictException>(async () =>
            await handler.Handle(
                new CompleteStockDocumentCommand(data.Document.Id),
                CancellationToken.None));

        Assert.Equal(5, Assert.Single(data.Inventories.Items).Quantity);
        Assert.Equal(StockDocumentStatus.Draft, data.Document.Status);
    }

    private static CompleteStockDocumentCommandHandler CreateHandler(TestDataResult data)
    {
        return new CompleteStockDocumentCommandHandler(
            new StockDocumentRepository(data.Document),
            data.Inventories,
            new WarehouseContext(data.Document.WarehouseId),
            new UnitOfWork());
    }

    private static TestDataResult TestData(
        StockDocumentType type,
        decimal inventoryQuantity,
        decimal reservedQuantity,
        decimal itemQuantity)
    {
        var warehouseId = Guid.NewGuid();
        var product = new Product { ProductId = Guid.NewGuid(), Name = "Produkt A" };
        var location = new Location
        {
            LocationId = Guid.NewGuid(),
            WarehouseId = warehouseId,
            LocationCode = "A-01"
        };
        var document = new StockDocument
        {
            Id = Guid.NewGuid(),
            Type = type,
            Status = StockDocumentStatus.Draft,
            WarehouseId = warehouseId,
            Items =
            [
                new StockDocumentItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    Product = product,
                    LocationId = location.LocationId,
                    Location = location,
                    Quantity = itemQuantity
                }
            ]
        };
        var inventory = new Inventory
        {
            InventoryId = Guid.NewGuid(),
            ProductId = product.ProductId,
            LocationId = location.LocationId,
            Quantity = inventoryQuantity,
            ReservedQuantity = reservedQuantity
        };

        return new TestDataResult(document, new ListRepository<Inventory>(inventory));
    }

    private sealed record TestDataResult(
        StockDocument Document,
        ListRepository<Inventory> Inventories);

    private sealed class StockDocumentRepository(StockDocument document)
        : IStockDocumentRepository
    {
        public Task<(IReadOnlyList<StockDocument> Items, int Total)> GetPageAsync(
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
            return Task.FromResult<(IReadOnlyList<StockDocument>, int)>(([document], 1));
        }

        public Task<StockDocument?> GetByIdAsync(
            Guid id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == document.Id ? document : null);
        }

        public Task AddAsync(
            StockDocument value,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ReplaceItemsAsync(
            StockDocument value,
            IReadOnlyCollection<StockDocumentItem> newItems,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(
            StockDocument value,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ListRepository<TEntity>(params TEntity[] items)
        : IRepository<TEntity> where TEntity : class
    {
        public List<TEntity> Items { get; } = [.. items];

        public Task<TEntity> AddAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(
            TEntity entity,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(
            TEntity entity,
            CancellationToken cancellationToken = default)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<TEntity?> FindByAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            FirstOrDefaultAsync(predicate, cancellationToken);

        public Task<IReadOnlyList<TEntity>> FilterByAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TEntity>>(
                Items.AsQueryable().Where(predicate).ToArray());

        public Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.AsQueryable().FirstOrDefault(predicate));

        public Task<IReadOnlyList<TEntity>> AllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TEntity>>(Items.ToArray());
    }

    private sealed class WarehouseContext(Guid warehouseId) : IWarehouseContext
    {
        public Guid? UserId => Guid.NewGuid();
        public bool IsAdministrator => false;
        public Guid? WarehouseId => warehouseId;
        public bool CanAccess(Guid value) => value == warehouseId;
    }

    private sealed class UnitOfWork : IUnitOfWork
    {
        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }
}
