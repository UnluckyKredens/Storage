using Mediator;

namespace MagazineAPIApplication.Modules.Locations;

public sealed record GetLocationsQuery() : IQuery<IReadOnlyList<LocationView>>;
