using System.Data;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIDomain.Entities;
using MagazineAPInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPI.Services;

public sealed class DocumentNumberGenerator(AppDbContext dbContext) : IDocumentNumberGenerator
{
    public async Task<string> GenerateAsync(
        Guid warehouseId,
        string documentType,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        if (warehouseId == Guid.Empty)
            throw new InvalidOperationException("Magazyn jest wymagany do numeracji dokumentu.");
        if (string.IsNullOrWhiteSpace(documentType))
            throw new InvalidOperationException("Typ dokumentu jest wymagany do numeracji.");

        var normalizedType = NormalizeDocumentType(documentType);
        var year = utcNow.Year;
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var warehouse = await dbContext.Warehouses
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.WarehouseId == warehouseId, cancellationToken)
                ?? throw new InvalidOperationException("Magazyn do numeracji dokumentu nie istnieje.");

            var sequence = await dbContext.DocumentNumberSequences
                .SingleOrDefaultAsync(
                    item =>
                        item.WarehouseId == warehouseId &&
                        item.DocumentType == normalizedType &&
                        item.Year == year,
                    cancellationToken);

            if (sequence is null)
            {
                sequence = new DocumentNumberSequence
                {
                    DocumentNumberSequenceId = Guid.NewGuid(),
                    WarehouseId = warehouseId,
                    DocumentType = normalizedType,
                    Year = year,
                    LastNumber = 0,
                    UpdatedOnUtc = utcNow
                };
                await dbContext.DocumentNumberSequences.AddAsync(sequence, cancellationToken);
            }

            sequence.LastNumber += 1;
            sequence.UpdatedOnUtc = utcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return $"{ResolveWarehouseCode(warehouse.Name)}/{normalizedType}/{year}/{sequence.LastNumber:000000}";
        });
    }

    private static string NormalizeDocumentType(string value)
    {
        var normalized = new string(value
            .Trim()
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .Take(20)
            .ToArray());

        return normalized.Length == 0
            ? throw new InvalidOperationException("Typ dokumentu nie zawiera znaków do numeracji.")
            : normalized;
    }

    private static string ResolveWarehouseCode(string warehouseName)
    {
        var letters = new string(warehouseName
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .Take(3)
            .ToArray());

        return letters.PadRight(3, 'X');
    }
}
