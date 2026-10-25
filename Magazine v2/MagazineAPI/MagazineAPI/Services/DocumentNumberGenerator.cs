using System.Data;
using System.Globalization;
using System.Text;
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
            var otherWarehouses = await dbContext.Warehouses
                .AsNoTracking()
                .Where(item => item.Id != warehouseId)
                .ToListAsync(ct);
            number = $"{WarehousePrefix(warehouse, otherWarehouses)}/{normalizedType}/{year}/{sequence.LastNumber:000000}";
        }, cancellationToken);
        return number;
    }

    // Readable prefix taken from the warehouse name ("Warszawa – Oddział" -> "WARSZAWA"); a short id suffix
    // is added only when another warehouse would produce the same prefix.
    private static string WarehousePrefix(Warehouse warehouse, IReadOnlyCollection<Warehouse> others)
    {
        var prefix = Slug(warehouse.Name);
        if (prefix.Length == 0)
            prefix = "MAG";
        return others.Any(other => Slug(other.Name) == prefix)
            ? $"{prefix}-{warehouse.Id.ToString("N")[..4].ToUpperInvariant()}"
            : prefix;
    }

    private static string Slug(string name)
    {
        var head = name.Split(new[] { " – ", " - ", " — " }, StringSplitOptions.None)[0];
        var letters = head.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Select(c => c == 'ł' ? 'l' : c == 'Ł' ? 'L' : c)
            .Where(char.IsLetterOrDigit)
            .Take(16);
        return new string(letters.ToArray()).ToUpperInvariant();
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
