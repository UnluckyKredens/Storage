using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Modules.Contractors;

internal static class ContractorMapper
{
    public static ContractorView ToView(Contractor item) => new(
            item.ContractorId,
            item.Name,
            item.TaxNumber,
            item.Type,
            item.Email,
            item.Phone,
            item.Address);
}
