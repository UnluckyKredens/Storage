using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class StorefrontOrder
{
    public Guid StorefrontOrderId { get; set; }
    public string Number { get; set; } = string.Empty;
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

    public Warehouse? AssignedWarehouse { get; set; }
    public ICollection<StorefrontOrderItem> Items { get; set; } = new List<StorefrontOrderItem>();
}
