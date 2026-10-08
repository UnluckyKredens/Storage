using Mediator;

namespace MagazineAPIApplication.Modules.Permissions;

public sealed record GetPermissionCatalogQuery() : IQuery<IReadOnlyList<PermissionView>>;
