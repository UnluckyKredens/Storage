using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class StorefrontOrder
{
    public byte[] RowVersion { get; set; } = [];
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string TrackingToken { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    public string? RequestFingerprint { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? AssignedWarehouseId { get; set; }
    public StorefrontOrderStatus Status { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? AcceptedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    public Guid? PackedByUserId { get; set; }
    public Guid? ShippedByUserId { get; set; }
    public DateTime? ShippedOnUtc { get; set; }
    public string? CarrierName { get; set; }
    public string? CarrierTrackingNumber { get; set; }

    public Warehouse? AssignedWarehouse { get; set; }
    public ICollection<StorefrontOrderItem> Items { get; set; } = new List<StorefrontOrderItem>();
}
