using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class StockMovement
{
    public Guid StockMovementId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid LocationId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? InventoryId { get; set; }
    public StockMovementType Type { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityChange { get; set; }
    public decimal QuantityAfter { get; set; }
    public decimal ReservedQuantityBefore { get; set; }
    public decimal ReservedQuantityChange { get; set; }
    public decimal ReservedQuantityAfter { get; set; }
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceNumber { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string? Notes { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Inventory? Inventory { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
