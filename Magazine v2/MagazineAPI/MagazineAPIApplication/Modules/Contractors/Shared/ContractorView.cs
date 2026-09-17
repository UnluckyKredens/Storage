using MagazineAPIDomain.Enums;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed record ContractorView(
    Guid Id,
    string Name,
    string TaxNumber,
    ContractorType Type,
    string? Email,
    string? Phone,
    string? Address);
