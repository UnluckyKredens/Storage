using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class WarehouseOperation
{
    public Guid WarehouseOperationId { get; set; }
    public string Number { get; set; } = string.Empty;
    public WarehouseOperationType Type { get; set; }
    public WarehouseOperationStatus Status { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public Guid CompletedByUserId { get; set; }
    public DateTime CompletedOnUtc { get; set; }
    public string? Notes { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public User CompletedByUser { get; set; } = null!;
    public ICollection<WarehouseOperationItem> Items { get; set; } = new List<WarehouseOperationItem>();
}
