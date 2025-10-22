using MagazineAPIDomain.Enums;
using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed record SaveContractorCommand(
    Guid? Id,
    string Name,
    string TaxNumber,
    ContractorType Type,
    string? Email,
    string? Phone,
    string? Address) : ICommand<ContractorView>;
