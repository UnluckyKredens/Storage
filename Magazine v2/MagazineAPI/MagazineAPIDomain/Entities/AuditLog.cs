namespace MagazineAPIDomain.Entities;

public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Summary { get; set; }
    public string? BeforeValuesJson { get; set; }
    public string? AfterValuesJson { get; set; }

    public User? User { get; set; }
}
