using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed record GetContractorsQuery() : IQuery<IReadOnlyList<ContractorView>>;
