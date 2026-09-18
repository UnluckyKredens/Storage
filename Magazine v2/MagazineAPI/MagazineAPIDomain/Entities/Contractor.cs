using MagazineAPIDomain.Enums;

namespace MagazineAPIDomain.Entities;

public class Contractor
{
    public Guid ContractorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public ContractorType Type { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
