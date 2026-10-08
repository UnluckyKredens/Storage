namespace MagazineAPIDomain.Entities;

public class Warehouse
{
    public Guid WarehouseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Description { get; set; }

    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Shipment> SourceShipments { get; set; } = new List<Shipment>();
    public ICollection<Shipment> DestinationShipments { get; set; } = new List<Shipment>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<StorefrontOrder> StorefrontOrders { get; set; } = new List<StorefrontOrder>();
}
