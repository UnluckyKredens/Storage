using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class PurchaseOrder
{
    public Guid PurchaseOrderId { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Guid ContractorId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedOnUtc { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public DateTime? ReceivedOnUtc { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? PaperDocumentNumber { get; set; }
    public string? Notes { get; set; }
    public PurchaseOrderStatus Status { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public Contractor Contractor { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? ReceivedByUser { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
