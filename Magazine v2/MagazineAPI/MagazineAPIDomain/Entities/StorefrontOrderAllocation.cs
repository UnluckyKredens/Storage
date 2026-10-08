namespace MagazineAPIDomain.Entities;

public class StorefrontOrderAllocation
{
    public Guid StorefrontOrderAllocationId { get; set; }
    public Guid StorefrontOrderItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid LocationId { get; set; }
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ConsumedQuantity { get; set; }

    public StorefrontOrderItem StorefrontOrderItem { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public Inventory Inventory { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
