using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public sealed class StockDocument
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public StockDocumentType Type { get; set; }
    public StockDocumentStatus Status { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? ContractorId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? ReceivedAtUtc { get; set; }
    public string? Notes { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public Contractor? Contractor { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? ReceivedByUser { get; set; }
    public ICollection<StockDocumentItem> Items { get; set; } = new List<StockDocumentItem>();
}
