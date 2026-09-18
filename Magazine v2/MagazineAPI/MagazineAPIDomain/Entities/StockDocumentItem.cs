namespace MagazineAPIDomain.Entities;

public sealed class StockDocumentItem
{
    public Guid Id { get; set; }
    public Guid StockDocumentId { get; set; }
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }

    public StockDocument StockDocument { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
