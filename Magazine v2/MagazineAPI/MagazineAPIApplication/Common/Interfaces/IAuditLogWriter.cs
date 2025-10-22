namespace MagazineAPIApplication.Common.Interfaces;

public sealed record AuditLogRecord(
    string Action,
    string EntityName,
    string? EntityId,
    string? Summary,
    string? BeforeValuesJson = null,
    string? AfterValuesJson = null);

public interface IAuditLogWriter
{
    Task RecordAsync(AuditLogRecord record, CancellationToken cancellationToken = default);
}
