using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPInfrastructure.Persistence.Repositories;

public sealed class RolePermissionRepository(AppDbContext dbContext) : IRolePermissionRepository
{
    public async Task<IReadOnlyList<Role>> GetRolesWithPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderBy(role => role.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Role?> GetRoleWithPermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .SingleOrDefaultAsync(role => role.Id == roleId, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceRolePermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var currentAssignments = await dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .ToListAsync(cancellationToken);

        dbContext.RolePermissions.RemoveRange(currentAssignments);
        dbContext.RolePermissions.AddRange(permissionIds.Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        }));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UserHasPermissionAsync(
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Id == userId
                    && (user.RoleId == RoleIds.Administrator
                        || user.Role.RolePermissions.Any(
                            rolePermission => rolePermission.Permission.Code == permissionCode)),
                cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roleId = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => (Guid?)user.RoleId)
            .SingleOrDefaultAsync(cancellationToken);

        if (roleId is null)
        {
            return [];
        }

        if (roleId == RoleIds.Administrator)
        {
            return await dbContext.Permissions
                .AsNoTracking()
                .OrderBy(permission => permission.Code)
                .Select(permission => permission.Code)
                .ToListAsync(cancellationToken);
        }

        return await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .OrderBy(rolePermission => rolePermission.Permission.Code)
            .Select(rolePermission => rolePermission.Permission.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionNamesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleId = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => (Guid?)user.RoleId)
            .SingleOrDefaultAsync(cancellationToken);

        if (roleId is null)
        {
            return [];
        }

        if (roleId == RoleIds.Administrator)
        {
            return await dbContext.Permissions
                .AsNoTracking()
                .OrderBy(permission => permission.Name)
                .Select(permission => permission.Name)
                .ToListAsync(cancellationToken);
        }

        return await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .OrderBy(rolePermission => rolePermission.Permission.Name)
            .Select(rolePermission => rolePermission.Permission.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserIsAdministratorAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Id == userId && user.RoleId == RoleIds.Administrator,
                cancellationToken);
    }

    public async Task<int> CountUsersInRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .CountAsync(user => user.RoleId == roleId, cancellationToken);
    }
}
