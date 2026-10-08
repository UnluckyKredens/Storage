namespace MagazineAPIDomain.Entities;

public class UnitOfMeasure
{
    public Guid UnitOfMeasureId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
