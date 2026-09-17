using Mediator;

namespace MagazineAPIApplication.Modules.Contractors;

public sealed record DeleteContractorCommand(Guid Id) : ICommand<bool>;
