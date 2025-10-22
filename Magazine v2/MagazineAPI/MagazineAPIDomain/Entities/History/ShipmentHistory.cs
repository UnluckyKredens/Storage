namespace MagazineAPIDomain.Entities.History;

public class ShipmentHistory
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public Guid SourceWarehouseId { get; init; }
    public Guid DestinationWarehouseId { get; init; }
    public Guid? UserId { get; init; }
    public DateTime CreatedOnUtc { get; init; }
    public string Details { get; init; } = string.Empty;
}
