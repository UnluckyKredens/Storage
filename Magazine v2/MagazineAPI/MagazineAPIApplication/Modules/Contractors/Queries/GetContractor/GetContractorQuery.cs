using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed record GetContractorQuery(Guid Id) : IQuery<ContractorView>;
