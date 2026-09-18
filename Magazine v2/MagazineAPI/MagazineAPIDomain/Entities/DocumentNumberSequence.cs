namespace MagazineAPIDomain.Entities;

public class DocumentNumberSequence
{
    public Guid DocumentNumberSequenceId { get; set; }
    public Guid WarehouseId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public int Year { get; set; }
    public int LastNumber { get; set; }
    public DateTime UpdatedOnUtc { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
}
