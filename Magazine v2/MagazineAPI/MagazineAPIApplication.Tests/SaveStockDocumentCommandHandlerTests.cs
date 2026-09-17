using System.Linq.Expressions;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.StockDocuments;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Xunit;

namespace MagazineAPIApplication.Tests;

public sealed class SaveStockDocumentCommandHandlerTests
{
    [Fact]
    public async Task CreatesShipmentToAnotherWarehouseWithoutContractor()
    {
        var data = TestData();
        var repository = new StockDocumentRepository();
        var handler = CreateHandler(data, repository);

        await handler.Handle(Command(data, data.Destination.WarehouseId), CancellationToken.None);

        Assert.NotNull(repository.Document);
        Assert.Equal(StockDocumentType.Shipment, repository.Document.Type);
        Assert.Equal(data.Destination.WarehouseId, repository.Document.DestinationWarehouseId);
        Assert.Null(repository.Document.ContractorId);
        Assert.Equal(StockDocumentStatus.Draft, repository.Document.Status);
    }

    [Fact]
    public async Task RejectsShipmentToItsSourceWarehouse()
    {
        var data = TestData();
        var handler = CreateHandler(data, new StockDocumentRepository());

        await Assert.ThrowsAsync<CommandValidationException>(async () =>
            await handler.Handle(
                Command(data, data.Source.WarehouseId),
                CancellationToken.None));
    }

    [Fact]
    public async Task ReplacesItemsWhenEditingDraft()
    {
        var data = TestData();
        var existingDocument = new StockDocument
        {
            Id = Guid.NewGuid(),
            Number = "WZ-TEST",
            Type = StockDocumentType.Shipment,
            Status = StockDocumentStatus.Draft,
            WarehouseId = data.Source.WarehouseId,
            DestinationWarehouseId = data.Destination.WarehouseId,
            CreatedByUserId = Guid.NewGuid(),
            Items =
            [
                new StockDocumentItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = data.Product.ProductId,
                    LocationId = data.Location.LocationId,
                    Quantity = 1
                }
            ]
        };
        var repository = new StockDocumentRepository(existingDocument);
        var handler = CreateHandler(data, repository);
        var command = Command(data, data.Destination.WarehouseId) with { Id = existingDocument.Id };

        await handler.Handle(command, CancellationToken.None);

        Assert.True(repository.ItemsWereReplaced);
        Assert.Single(existingDocument.Items);
        Assert.Equal(2, existingDocument.Items.Single().Quantity);
    }

    private static SaveStockDocumentCommandHandler CreateHandler(
        TestDataResult data,
        StockDocumentRepository repository)
    {
        return new SaveStockDocumentCommandHandler(
            repository,
            new ListRepository<Warehouse>(data.Source, data.Destination),
            new ListRepository<Contractor>(),
            new ListRepository<Product>(data.Product),
            new ListRepository<Location>(data.Location),
            new WarehouseContext(data.Source.WarehouseId));
    }

    private static SaveStockDocumentCommand Command(TestDataResult data, Guid destinationId)
    {
        return new SaveStockDocumentCommand(
            null,
            StockDocumentType.Shipment,
            data.Source.WarehouseId,
            null,
            destinationId,
            null,
            [new SaveStockDocumentItem(data.Product.ProductId, data.Location.LocationId, 2)]);
    }

    private static TestDataResult TestData()
    {
        var source = new Warehouse { WarehouseId = Guid.NewGuid(), Name = "Warszawa" };
        var destination = new Warehouse { WarehouseId = Guid.NewGuid(), Name = "Poznań" };
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Name = "Produkt A",
            Sku = "SKU-1",
            IsActive = true
        };
        var location = new Location
        {
            LocationId = Guid.NewGuid(),
            WarehouseId = source.WarehouseId,
            LocationCode = "A-01"
        };
        return new TestDataResult(source, destination, product, location);
    }

    private sealed record TestDataResult(
        Warehouse Source,
        Warehouse Destination,
        Product Product,
        Location Location);

    private sealed class StockDocumentRepository(StockDocument? document = null)
        : IStockDocumentRepository
    {
        public StockDocument? Document { get; private set; } = document;
        public bool ItemsWereReplaced { get; private set; }

        public Task<(IReadOnlyList<StockDocument> Items, int Total)> GetPageAsync(
            StockDocumentType? type,
            StockDocumentStatus? status,
            Guid? warehouseId,
            string search,
            string sortBy,
            bool descending,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<StockDocument>, int)>(([], 0));

        public Task<StockDocument?> GetByIdAsync(
            Guid id,
            bool trackChanges = false,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Document?.Id == id ? Document : null);

        public Task AddAsync(
            StockDocument document,
            CancellationToken cancellationToken = default)
        {
            Document = document;
            return Task.CompletedTask;
        }

        public Task ReplaceItemsAsync(
            StockDocument document,
            IReadOnlyCollection<StockDocumentItem> newItems,
            CancellationToken cancellationToken = default)
        {
            document.Items = [.. newItems];
            ItemsWereReplaced = true;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteAsync(
            StockDocument document,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ListRepository<TEntity>(params TEntity[] items)
        : IRepository<TEntity> where TEntity : class
    {
        private List<TEntity> Items { get; } = [.. items];

        public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
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
        public Guid? UserId { get; } = Guid.NewGuid();
        public bool IsAdministrator => false;
        public Guid? WarehouseId => warehouseId;
        public bool CanAccess(Guid value) => value == warehouseId;
    }
}
