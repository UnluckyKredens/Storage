namespace MagazineAPIDomain.Entities;

public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal? OptimumQuantity { get; set; }
    public bool IsActive { get; set; }

    public Category Category { get; set; } = null!;
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<StorefrontOrderItem> StorefrontOrderItems { get; set; } = new List<StorefrontOrderItem>();
}
