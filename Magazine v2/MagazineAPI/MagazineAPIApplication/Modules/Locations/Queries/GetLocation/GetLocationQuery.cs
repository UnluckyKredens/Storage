using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed record GetLocationQuery(Guid Id) : IQuery<LocationView>;
