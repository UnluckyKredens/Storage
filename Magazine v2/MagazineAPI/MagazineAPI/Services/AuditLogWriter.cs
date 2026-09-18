using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;

namespace MagazineAPI.Services;

public sealed class AuditLogWriter(
    IRepository<AuditLog> auditLogRepository,
    ICurrentUser currentUser) : IAuditLogWriter
{
    public async Task RecordAsync(AuditLogRecord record, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            CreatedOnUtc = DateTime.UtcNow,
            UserId = currentUser.UserId,
            Action = record.Action,
            EntityName = record.EntityName,
            EntityId = record.EntityId,
            Summary = record.Summary,
            BeforeValuesJson = record.BeforeValuesJson,
            AfterValuesJson = record.AfterValuesJson
        };

        await auditLogRepository.AddAsync(auditLog, cancellationToken);
    }
}
