using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using Mediator;

namespace MagazineAPIApplication.Modules.StockDocuments;

public sealed class SaveStockDocumentCommandHandler(
    IStockDocumentRepository documentRepository,
    IRepository<Warehouse> warehouseRepository,
    IRepository<Contractor> contractorRepository,
    IRepository<Product> productRepository,
    IRepository<Location> locationRepository,
    IWarehouseContext warehouseContext)
    : ICommandHandler<SaveStockDocumentCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        SaveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        ValidateBasicData(command);

        var warehouse = await warehouseRepository.FirstOrDefaultAsync(
            item => item.WarehouseId == command.WarehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Wybrany magazyn nie istnieje.");
        if (!warehouseContext.CanAccess(warehouse.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do wybranego magazynu.");

        await ValidateDestination(command, cancellationToken);
        await ValidateItems(command, cancellationToken);

        if (command.Id is null)
            return await Create(command, cancellationToken);

        var document = await documentRepository.GetByIdAsync(
            command.Id.Value,
            trackChanges: true,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Dokument nie został znaleziony.");
        if (!warehouseContext.CanAccess(document.WarehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu dokumentu.");
        if (document.Status != StockDocumentStatus.Draft)
            throw new ResourceConflictException("Można edytować tylko dokument w statusie Szkic.");
        if (document.Type != command.Type)
            throw new CommandValidationException("Nie można zmienić typu istniejącego dokumentu.");

        document.WarehouseId = command.WarehouseId;
        document.ContractorId = command.ContractorId;
        document.DestinationWarehouseId = command.DestinationWarehouseId;
        document.Notes = NormalizeNotes(command.Notes);
        var newItems = command.Items
            .Select(item => CreateItem(document.Id, item))
            .ToList();

        await documentRepository.ReplaceItemsAsync(document, newItems, cancellationToken);
        return document.Id;
    }

    private async Task<Guid> Create(
        SaveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var userId = warehouseContext.UserId
            ?? throw new ForbiddenOperationException("Brak identyfikatora użytkownika.");
        var id = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;
        var prefix = command.Type == StockDocumentType.Receipt ? "PZ" : "WZ";
        var document = new StockDocument
        {
            Id = id,
            Number = $"{prefix}-{createdAtUtc:yyyyMMdd-HHmmss}-{id.ToString()[..8].ToUpperInvariant()}",
            Type = command.Type,
            Status = StockDocumentStatus.Draft,
            WarehouseId = command.WarehouseId,
            ContractorId = command.ContractorId,
            DestinationWarehouseId = command.DestinationWarehouseId,
            CreatedByUserId = userId,
            CreatedAtUtc = createdAtUtc,
            Notes = NormalizeNotes(command.Notes),
            Items = command.Items.Select(item => CreateItem(id, item)).ToList()
        };

        await documentRepository.AddAsync(document, cancellationToken);
        return document.Id;
    }

    private async Task ValidateItems(
        SaveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var pairs = new HashSet<(Guid ProductId, Guid LocationId)>();
        foreach (var item in command.Items)
        {
            if (item.ProductId == Guid.Empty || item.LocationId == Guid.Empty)
                throw new CommandValidationException("Produkt i lokalizacja są wymagane.");
            if (item.Quantity <= 0)
                throw new CommandValidationException("Ilość pozycji musi być większa od zera.");
            if (!pairs.Add((item.ProductId, item.LocationId)))
                throw new CommandValidationException("Produkt w tej samej lokalizacji może wystąpić tylko raz.");

            var product = await productRepository.FirstOrDefaultAsync(
                product => product.ProductId == item.ProductId,
                cancellationToken);
            if (product is null || !product.IsActive)
                throw new CommandValidationException("Wybrany produkt nie istnieje lub jest nieaktywny.");

            var location = await locationRepository.FirstOrDefaultAsync(
                location => location.LocationId == item.LocationId,
                cancellationToken)
                ?? throw new CommandValidationException("Wybrana lokalizacja nie istnieje.");
            if (location.WarehouseId != command.WarehouseId)
                throw new CommandValidationException("Wszystkie lokalizacje muszą należeć do magazynu dokumentu.");
        }
    }

    private static void ValidateBasicData(SaveStockDocumentCommand command)
    {
        if (!Enum.IsDefined(command.Type))
            throw new CommandValidationException("Nieprawidłowy typ dokumentu.");
        if (command.WarehouseId == Guid.Empty)
            throw new CommandValidationException("Magazyn jest wymagany.");
        if (command.Items is null || command.Items.Count == 0)
            throw new CommandValidationException("Dokument musi zawierać co najmniej jedną pozycję.");
        if (command.Items.Count > 100)
            throw new CommandValidationException("Dokument może zawierać maksymalnie 100 pozycji.");
        if (command.Notes?.Length > 1000)
            throw new CommandValidationException("Uwagi mogą mieć maksymalnie 1000 znaków.");
    }

    private async Task ValidateDestination(
        SaveStockDocumentCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Type == StockDocumentType.Receipt)
        {
            if (command.ContractorId is null || command.ContractorId == Guid.Empty)
                throw new CommandValidationException("Dostawca jest wymagany.");
            if (command.DestinationWarehouseId is not null)
                throw new CommandValidationException("Przyjęcie PZ nie może mieć magazynu docelowego.");

            var contractor = await contractorRepository.FirstOrDefaultAsync(
                item => item.ContractorId == command.ContractorId,
                cancellationToken)
                ?? throw new CommandValidationException("Wybrany dostawca nie istnieje.");
            if (contractor.Type is not (ContractorType.Supplier or ContractorType.Both))
                throw new CommandValidationException("Dla przyjęcia wybierz dostawcę.");
            return;
        }

        if (command.ContractorId is not null)
            throw new CommandValidationException("Wysyłka międzyoddziałowa nie może mieć kontrahenta.");
        if (command.DestinationWarehouseId is null || command.DestinationWarehouseId == Guid.Empty)
            throw new CommandValidationException("Magazyn docelowy jest wymagany.");
        if (command.DestinationWarehouseId == command.WarehouseId)
            throw new CommandValidationException("Magazyn docelowy musi być innym oddziałem.");

        _ = await warehouseRepository.FirstOrDefaultAsync(
            item => item.WarehouseId == command.DestinationWarehouseId,
            cancellationToken)
            ?? throw new CommandValidationException("Magazyn docelowy nie istnieje.");
    }

    private static StockDocumentItem CreateItem(
        Guid documentId,
        SaveStockDocumentItem item)
    {
        return new StockDocumentItem
        {
            Id = Guid.NewGuid(),
            StockDocumentId = documentId,
            ProductId = item.ProductId,
            LocationId = item.LocationId,
            Quantity = item.Quantity
        };
    }

    private static string? NormalizeNotes(string? notes)
    {
        return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
