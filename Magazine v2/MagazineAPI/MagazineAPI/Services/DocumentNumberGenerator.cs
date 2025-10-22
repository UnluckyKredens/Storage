using System.Data;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIDomain.Entities;
using MagazineAPInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPI.Services;

public sealed class DocumentNumberGenerator(AppDbContext dbContext, MagazineAPIDomain.Repositories.IUnitOfWork unitOfWork) : IDocumentNumberGenerator
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
        string number = string.Empty;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var warehouse = await dbContext.Warehouses
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == warehouseId, cancellationToken)
                ?? throw new InvalidOperationException("Magazyn do numeracji dokumentu nie istnieje.");

            // Take the update lock on the first read; read-then-update would deadlock concurrent requests.
            var sequence = await dbContext.DocumentNumberSequences
                .FromSqlInterpolated($"SELECT * FROM [DocumentNumberSequences] WITH (UPDLOCK, ROWLOCK) WHERE [WarehouseId] = {warehouseId} AND [DocumentType] = {normalizedType} AND [Year] = {year}")
                .SingleOrDefaultAsync(cancellationToken);

            if (sequence is null)
            {
                sequence = new DocumentNumberSequence
                {
                    Id = Guid.NewGuid(),
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
            var otherWarehouseIds = await dbContext.Warehouses
                .AsNoTracking()
                .Where(item => item.Id != warehouseId)
                .Select(item => item.Id)
                .ToListAsync(ct);
            number = $"{WarehousePrefix(warehouse.Id, otherWarehouseIds)}/{normalizedType}/{year}/{sequence.LastNumber:000000}";
        }, cancellationToken);
        return number;
    }

    // Short, stable and readable warehouse prefix; widened only if two warehouses would share it.
    private static string WarehousePrefix(Guid warehouseId, IReadOnlyCollection<Guid> otherWarehouseIds)
    {
        var hex = warehouseId.ToString("N").ToUpperInvariant();
        for (var length = 6; length < hex.Length; length += 2)
        {
            var prefix = hex[..length];
            if (otherWarehouseIds.All(other => !other.ToString("N").StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                return $"MAG-{prefix}";
        }
        return $"MAG-{hex}";
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

}
