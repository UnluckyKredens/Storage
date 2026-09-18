using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPIApplication.Modules.AuditLogs;

public sealed class GetAuditLogsQueryHandler(
    IRepository<AuditLog> auditLogRepository,
    IRepository<User> userRepository) : IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogView>>
{
    public async ValueTask<IReadOnlyList<AuditLogView>> Handle(
        GetAuditLogsQuery query,
        CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(query.Limit, 1, 1000);
        var logsQuery = auditLogRepository.Query();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
            logsQuery = logsQuery.Where(log => log.EntityName == query.EntityName.Trim());
        if (!string.IsNullOrWhiteSpace(query.EntityId))
            logsQuery = logsQuery.Where(log => log.EntityId == query.EntityId.Trim());

        var orderedLogs = await logsQuery
            .OrderByDescending(log => log.CreatedOnUtc)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
        var userIds = orderedLogs
            .Where(log => log.UserId is not null)
            .Select(log => log.UserId!.Value)
            .ToHashSet();
        var users = await userRepository.Query()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        return orderedLogs
            .Select(log =>
            {
                var user = log.UserId is null ? null : users.GetValueOrDefault(log.UserId.Value);
                return new AuditLogView(
                    log.AuditLogId,
                    log.CreatedOnUtc,
                    log.UserId,
                    user is null ? null : $"{user.FirstName} {user.LastName}",
                    log.Action,
                    log.EntityName,
                    log.EntityId,
                    log.Summary,
                    log.BeforeValuesJson,
                    log.AfterValuesJson);
            })
            .ToArray();
    }
}
