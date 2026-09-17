using System.Linq.Expressions;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Security;
using MagazineAPIApplication.Modules.Authentication;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Repositories;
using Xunit;

namespace MagazineAPIApplication.Tests;

public sealed class UpdateOwnAccountCommandHandlerTests
{
    private readonly Guid _roleId = Guid.NewGuid();
    private readonly User _user;
    private readonly ListRepository<User> _users;
    private readonly UpdateOwnAccountCommandHandler _handler;

    public UpdateOwnAccountCommandHandlerTests()
    {
        _user = new User
        {
            Id = Guid.NewGuid(),
            Login = "anna",
            FirstName = "Anna",
            LastName = "Kowalska",
            Email = "anna@example.com",
            PasswordHash = "hash:old-password",
            RoleId = _roleId
        };
        _users = new ListRepository<User>(_user);
        var roles = new ListRepository<Role>(new Role { Id = _roleId, Name = "Pracownik" });
        var warehouses = new ListRepository<Warehouse>();
        _handler = new UpdateOwnAccountCommandHandler(
            _users, roles, warehouses, new FakePasswordHashingService(), new FakeJwtTokenService());
    }

    [Fact]
    public async Task UpdatesOwnDataWithoutChangingRoleAndIssuesNewToken()
    {
        var result = await _handler.Handle(Command(firstName: "Maria", newPassword: "new-password",
            currentPassword: "old-password"), CancellationToken.None);

        Assert.Equal("Maria", _user.FirstName);
        Assert.Equal("hash:new-password", _user.PasswordHash);
        Assert.Equal(_roleId, _user.RoleId);
        Assert.Equal("anna:Maria:Pracownik", result.Token);
        Assert.Equal("Maria", result.User.FirstName);
        Assert.Equal(1, _users.UpdateCount);
    }

    [Fact]
    public async Task ChangesNameWithoutChangingPassword()
    {
        var result = await _handler.Handle(Command(firstName: "Maria"), CancellationToken.None);

        Assert.Equal("Maria", result.User.FirstName);
        Assert.Equal("hash:old-password", _user.PasswordHash);
        Assert.Equal(_roleId, _user.RoleId);
    }

    [Fact]
    public async Task RejectsWrongCurrentPasswordWithoutSaving()
    {
        await Assert.ThrowsAsync<CommandValidationException>(async () =>
            await _handler.Handle(Command(firstName: "Maria", newPassword: "new-password",
                currentPassword: "wrong-password"), CancellationToken.None));

        Assert.Equal("Anna", _user.FirstName);
        Assert.Equal("hash:old-password", _user.PasswordHash);
        Assert.Equal(0, _users.UpdateCount);
    }

    [Fact]
    public async Task RejectsAnotherUsersLogin()
    {
        _users.Items.Add(new User
        {
            Id = Guid.NewGuid(),
            Login = "maria",
            FirstName = "Maria",
            LastName = "Nowak",
            Email = "maria@example.com",
            PasswordHash = "hash:password",
            RoleId = _roleId
        });

        await Assert.ThrowsAsync<ResourceConflictException>(async () =>
            await _handler.Handle(Command(login: "maria"), CancellationToken.None));

        Assert.Equal("anna", _user.Login);
        Assert.Equal(0, _users.UpdateCount);
    }

    private UpdateOwnAccountCommand Command(string login = "anna", string firstName = "Anna",
        string? currentPassword = null, string? newPassword = null)
    {
        return new UpdateOwnAccountCommand(_user.Id, login, firstName, "Kowalska",
            "anna@example.com", currentPassword, newPassword);
    }

    private sealed class ListRepository<TEntity>(params TEntity[] items) : IRepository<TEntity>
        where TEntity : class
    {
        public List<TEntity> Items { get; } = [.. items];
        public int UpdateCount { get; private set; }

        public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<TEntity?> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
            => FirstOrDefaultAsync(predicate, cancellationToken);

        public Task<IReadOnlyList<TEntity>> FilterByAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TEntity>>(Items.AsQueryable().Where(predicate).ToArray());

        public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.AsQueryable().FirstOrDefault(predicate));

        public Task<IReadOnlyList<TEntity>> AllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TEntity>>(Items.ToArray());
    }

    private sealed class FakePasswordHashingService : IPasswordHashingService
    {
        public string HashPassword(string password) => $"hash:{password}";
        public bool VerifyPassword(string password, string passwordHash) =>
            passwordHash == HashPassword(password);
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public JwtTokenResult CreateToken(User user, string roleName) =>
            new($"{user.Login}:{user.FirstName}:{roleName}", DateTime.UtcNow.AddHours(1));
    }
}
