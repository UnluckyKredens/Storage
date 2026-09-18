using MagazineAPIDomain.Entities;

namespace MagazineAPIDomain.Repositories;

public interface IRolePermissionRepository
{
    Task<IReadOnlyList<Role>> GetRolesWithPermissionsAsync(
        CancellationToken cancellationToken = default);

    Task<Role?> GetRoleWithPermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetPermissionsAsync(
        CancellationToken cancellationToken = default);

    Task ReplaceRolePermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default);

    Task<bool> UserHasPermissionAsync(
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default);

    Task<bool> UserIsAdministratorAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> CountUsersInRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserPermissionNamesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
