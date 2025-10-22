namespace MagazineAPIDomain.Entities;

public sealed class HistoryOutboxMessage
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
}
