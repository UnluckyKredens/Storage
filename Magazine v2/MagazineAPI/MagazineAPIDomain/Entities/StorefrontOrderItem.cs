namespace MagazineAPIDomain.Entities;

public class StorefrontOrderItem
{
    public Guid StorefrontOrderItemId { get; set; }
    public Guid StorefrontOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public StorefrontOrder StorefrontOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<StorefrontOrderAllocation> Allocations { get; set; } = new List<StorefrontOrderAllocation>();
}
