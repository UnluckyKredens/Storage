namespace MagazineAPIDomain.Entities;

public class Location
{
    public Guid LocationId { get; set; }
    public Guid WarehouseId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
