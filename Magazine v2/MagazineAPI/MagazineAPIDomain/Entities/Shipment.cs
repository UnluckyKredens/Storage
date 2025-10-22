using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class Shipment
{
    public byte[] RowVersion { get; set; } = [];
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid SourceWarehouseId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedOnUtc { get; set; }
    public Guid? PackedByUserId { get; set; }
    public DateTime? PackedOnUtc { get; set; }
    public Guid? SentByUserId { get; set; }
    public DateTime? SentOnUtc { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public DateTime? ReceivedOnUtc { get; set; }
    public ShipmentStatus Status { get; set; }

    public Warehouse SourceWarehouse { get; set; } = null!;
    public Warehouse DestinationWarehouse { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? PackedByUser { get; set; }
    public User? SentByUser { get; set; }
    public User? ReceivedByUser { get; set; }
    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
}
