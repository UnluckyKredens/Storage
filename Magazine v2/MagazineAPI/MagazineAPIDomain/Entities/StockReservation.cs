using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class StockReservation
{
    public Guid StockReservationId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid LocationId { get; set; }
    public Guid ProductId { get; set; }
    public Guid InventoryId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReleasedQuantity { get; set; }
    public StockReservationStatus Status { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public Guid? SourceItemId { get; set; }
    public string? SourceNumber { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? ClosedOnUtc { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Inventory Inventory { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
