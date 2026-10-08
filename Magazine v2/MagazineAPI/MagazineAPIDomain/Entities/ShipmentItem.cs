namespace MagazineAPIDomain.Entities;

public class ShipmentItem
{
    public Guid ShipmentItemId { get; set; }
    public Guid ShipmentId { get; set; }
    public Guid ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public Shipment Shipment { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
