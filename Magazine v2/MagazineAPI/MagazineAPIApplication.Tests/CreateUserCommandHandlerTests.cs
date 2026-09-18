using System.Linq.Expressions;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.Users;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Xunit;

namespace MagazineAPIApplication.Tests;

public sealed class CreateUserCommandHandlerTests
{
    private readonly Guid _actorUserId = Guid.NewGuid();
    private readonly Warehouse _warehouse = new()
    {
        WarehouseId = Guid.NewGuid(),
        Name = "Magazyn Warszawa"
    };
    private readonly ListRepository<User> _users = new();
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        var roles = new ListRepository<Role>(
            new Role { Id = RoleIds.Administrator, Name = "Administrator" },
            new Role { Id = RoleIds.User, Name = "Pracownik" });
        var warehouses = new ListRepository<Warehouse>(_warehouse);

        _handler = new CreateUserCommandHandler(
            _users,
            roles,
            warehouses,
            new AdministratorRolePermissionRepository(),
            new FakePasswordHashingService(),
            new AdministratorWarehouseContext(_actorUserId));
    }

    [Fact]
    public async Task RequiresWarehouseForNonAdministrator()
    {
        await Assert.ThrowsAsync<CommandValidationException>(async () =>
            await _handler.Handle(Command(RoleIds.User, null), CancellationToken.None));

        Assert.Empty(_users.Items);
    }

    [Fact]
    public async Task AssignsWarehouseToNonAdministrator()
    {
        var result = await _handler.Handle(
            Command(RoleIds.User, _warehouse.WarehouseId),
            CancellationToken.None);

        var user = Assert.Single(_users.Items);
        Assert.Equal(_warehouse.WarehouseId, user.WarehouseId);
        Assert.Equal(_warehouse.WarehouseId, result.WarehouseId);
        Assert.Equal(_warehouse.Name, result.WarehouseName);
    }

    [Fact]
    public async Task DoesNotAssignWarehouseToAdministrator()
    {
        var result = await _handler.Handle(
            Command(RoleIds.Administrator, _warehouse.WarehouseId),
            CancellationToken.None);

        Assert.Null(Assert.Single(_users.Items).WarehouseId);
        Assert.Null(result.WarehouseId);
    }

    private CreateUserCommand Command(Guid roleId, Guid? warehouseId)
    {
        return new CreateUserCommand(
            "nowy.uzytkownik",
            "Nowy",
            "Użytkownik",
            "nowy@example.com",
            "bezpieczne-haslo",
            roleId,
            warehouseId,
            _actorUserId);
    }

    private sealed class ListRepository<TEntity>(params TEntity[] items) : IRepository<TEntity>
        where TEntity : class
    {
        public List<TEntity> Items { get; } = [.. items];

        public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<TEntity?> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            FirstOrDefaultAsync(predicate, cancellationToken);

        public Task<IReadOnlyList<TEntity>> FilterByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TEntity>>(Items.AsQueryable().Where(predicate).ToArray());

        public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.AsQueryable().FirstOrDefault(predicate));

        public Task<IReadOnlyList<TEntity>> AllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TEntity>>(Items.ToArray());
    }

    private sealed class AdministratorRolePermissionRepository : IRolePermissionRepository
    {
        public Task<bool> UserIsAdministratorAsync(Guid userId,
            CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<IReadOnlyList<Role>> GetRolesWithPermissionsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Role>>([]);

        public Task<Role?> GetRoleWithPermissionsAsync(Guid roleId,
            CancellationToken cancellationToken = default) => Task.FromResult<Role?>(null);

        public Task<IReadOnlyList<Permission>> GetPermissionsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Permission>>([]);

        public Task ReplaceRolePermissionsAsync(Guid roleId,
            IReadOnlyCollection<Guid> permissionIds,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode,
            CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task<int> CountUsersInRoleAsync(Guid roleId,
            CancellationToken cancellationToken = default) => Task.FromResult(1);

        public Task<IReadOnlyList<string>> GetUserPermissionCodesAsync(Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public Task<IReadOnlyList<string>> GetUserPermissionNamesAsync(Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);

    }

    private sealed class FakePasswordHashingService : IPasswordHashingService
    {
        public string HashPassword(string password) => $"hash:{password}";

        public bool VerifyPassword(string password, string passwordHash) =>
            passwordHash == HashPassword(password);
    }

    private sealed class AdministratorWarehouseContext(Guid userId) : IWarehouseContext
    {
        public Guid? UserId => userId;
        public bool IsAdministrator => true;
        public Guid? WarehouseId => null;
        public bool CanAccess(Guid warehouseId) => true;
    }
}
