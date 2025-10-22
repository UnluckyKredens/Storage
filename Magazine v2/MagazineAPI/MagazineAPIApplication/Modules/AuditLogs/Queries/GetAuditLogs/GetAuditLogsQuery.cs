using Mediator;

namespace MagazineAPIApplication.Modules.AuditLogs;

public sealed record GetAuditLogsQuery(
    string? EntityName = null,
    string? EntityId = null,
    int Limit = 500) : IQuery<IReadOnlyList<AuditLogView>>;
