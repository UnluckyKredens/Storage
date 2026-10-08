namespace MagazineAPIDomain.Entities;

public class Inventory
{
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; private set; }

    public Product Product { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
