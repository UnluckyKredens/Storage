namespace MagazineAPIDomain.Entities.History;

public class InventoryHistory
{
    public Guid Id { get; set; }
    public Guid? ItemId { get; init; }
    public Guid ProductId { get; init; }
    public Guid LocationId { get; init; }
    public decimal Quantity { get; init; }
    public decimal ReservedQuantity { get; init; }
    public Guid AggregateId { get; init; }
    public decimal QuantityChange { get; init; }
    public DateTime CreatedOn { get; init; }
}