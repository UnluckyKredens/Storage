namespace MagazineAPIDomain.Entities;

public class WarehouseOperationItem
{
    public Guid WarehouseOperationItemId { get; set; }
    public Guid WarehouseOperationId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SourceLocationId { get; set; }
    public Guid? DestinationLocationId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? TargetQuantity { get; set; }

    public WarehouseOperation WarehouseOperation { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Location? SourceLocation { get; set; }
    public Location? DestinationLocation { get; set; }
}
