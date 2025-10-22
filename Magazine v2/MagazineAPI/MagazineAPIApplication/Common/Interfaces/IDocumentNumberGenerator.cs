namespace MagazineAPIApplication.Common.Interfaces;

public interface IDocumentNumberGenerator
{
    Task<string> GenerateAsync(
        Guid warehouseId,
        string documentType,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
