namespace MagazineAPIApplication.Modules.AuditLogs;

public sealed record AuditLogView(
    Guid Id,
    DateTime CreatedOnUtc,
    Guid? UserId,
    string? UserName,
    string Action,
    string EntityName,
    string? EntityId,
    string? Summary,
    string? BeforeValuesJson,
    string? AfterValuesJson);
