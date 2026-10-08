using MagazineWarehouseBot.Models;
using Microsoft.Extensions.Options;

namespace MagazineWarehouseBot.Simulation;

public sealed class BotControlState
{
    private readonly object _gate = new();
    private readonly List<BotActorState> _actors = [];

    public BotControlState(IOptions<WarehouseBotOptions> options)
    {
        ConfigureAccounts(ReadAccounts(options.Value.Accounts));
        IsRunning = options.Value.AutoStart && _actors.Any(x => x.IsConfigured);
    }

    public bool IsRunning { get; private set; }

    public void Start()
    {
        lock (_gate)
        {
            IsRunning = true;
        }
    }

    public void Stop()
    {
        lock (_gate)
        {
            IsRunning = false;
        }
    }

    public void ConfigureAccounts(IReadOnlyList<BotAccountOptions> accounts)
    {
        lock (_gate)
        {
            _actors.Clear();
            foreach (var account in ReadAccounts(accounts).Where(x => x.IsConfigured))
            {
                _actors.Add(new BotActorState(account));
            }
        }
    }

    public void EnsureAccounts(IReadOnlyList<BotAccountOptions> accounts)
    {
        lock (_gate)
        {
            var existingLogins = _actors
                .Select(actor => actor.Login)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var account in ReadAccounts(accounts).Where(x => x.IsConfigured))
            {
                if (existingLogins.Add(account.Login))
                {
                    _actors.Add(new BotActorState(account));
                }
            }
        }
    }

    public IReadOnlyList<BotActorState> GetActors()
    {
        lock (_gate)
        {
            return _actors.ToArray();
        }
    }

    public bool TryGetActor(Guid id, out BotActorState? actor)
    {
        lock (_gate)
        {
            actor = _actors.FirstOrDefault(x => x.Id == id);
            return actor is not null;
        }
    }

    public void MarkLogin(BotActorState actor, string token, UserView? user, IReadOnlySet<string> permissions)
    {
        lock (_gate)
        {
            actor.Token = token;
            actor.User = user;
            actor.Permissions = permissions;
            actor.LastLoginUtc = DateTimeOffset.UtcNow;
            actor.LastError = null;
        }
    }

    public void MarkAction(BotActorState actor, string action, string? error = null)
    {
        lock (_gate)
        {
            actor.LastAction = action;
            actor.LastActionUtc = DateTimeOffset.UtcNow;
            actor.LastError = error;
            actor.ActionsCount++;
        }
    }

    public void MarkWaiting(BotActorState actor, string action, string? error = null)
    {
        lock (_gate)
        {
            actor.LastAction = action;
            actor.LastActionUtc = DateTimeOffset.UtcNow;
            actor.LastError = error;
        }
    }

    public void ScheduleAction(BotActorState actor, string action, DateTimeOffset dueAt)
    {
        lock (_gate)
        {
            actor.NextActionsUtc[action] = dueAt;
        }
    }

    private static IReadOnlyList<BotAccountOptions> ReadAccounts(IReadOnlyList<BotAccountOptions> configured)
    {
        var accounts = configured
            .Select(x => new BotAccountOptions
            {
                Name = x.Name,
                Role = x.Role,
                Login = x.Login,
                Password = x.Password,
                WarehouseId = x.WarehouseId
            })
            .ToList();

        ApplyEnvironment(accounts, "BOT_ADMIN", "Administrator", BotRole.Administrator);
        ApplyEnvironment(accounts, "BOT_WORKER1", "Pracownik 1", BotRole.Worker);
        ApplyEnvironment(accounts, "BOT_WORKER2", "Pracownik 2", BotRole.Worker);
        ApplyEnvironment(accounts, "BOT_MANAGER", "Kierownik", BotRole.Manager);

        return accounts;
    }

    private static void ApplyEnvironment(
        List<BotAccountOptions> accounts,
        string prefix,
        string defaultName,
        BotRole role)
    {
        var login = Environment.GetEnvironmentVariable($"{prefix}_LOGIN");
        var password = Environment.GetEnvironmentVariable($"{prefix}_PASSWORD");
        if (string.IsNullOrWhiteSpace(login) && string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var account = accounts.FirstOrDefault(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase));
        if (account is null)
        {
            account = new BotAccountOptions();
            accounts.Add(account);
        }

        account.Name = Environment.GetEnvironmentVariable($"{prefix}_NAME") ?? account.Name;
        account.Login = login ?? account.Login;
        account.Password = password ?? account.Password;
        account.Role = role;

        if (string.IsNullOrWhiteSpace(account.Name))
        {
            account.Name = defaultName;
        }

        var warehouseId = Environment.GetEnvironmentVariable($"{prefix}_WAREHOUSE_ID");
        if (Guid.TryParse(warehouseId, out var parsedWarehouseId))
        {
            account.WarehouseId = parsedWarehouseId;
        }
    }
}

public sealed class BotActorState(BotAccountOptions account)
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = string.IsNullOrWhiteSpace(account.Name) ? account.Login : account.Name;
    public BotRole Role { get; } = account.Role;
    public string Login { get; } = account.Login;
    public string Password { get; } = account.Password;
    public Guid? PreferredWarehouseId { get; } = account.WarehouseId;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);

    public string? Token { get; set; }
    public UserView? User { get; set; }
    public IReadOnlySet<string> Permissions { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public DateTimeOffset? LastLoginUtc { get; set; }
    public DateTimeOffset? LastActionUtc { get; set; }
    public string? LastAction { get; set; }
    public string? LastError { get; set; }
    public int ActionsCount { get; set; }
    public Dictionary<string, DateTimeOffset> NextActionsUtc { get; } = [];
    public bool ScheduleInitialized { get; set; }

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission) || string.Equals(User?.RoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
}

public sealed record BotActorSnapshot(
    Guid Id,
    string Name,
    string Login,
    string Role,
    string? Warehouse,
    bool IsOnline,
    DateTimeOffset? NextActionUtc,
    string? NextAction,
    DateTimeOffset? LastLoginUtc,
    DateTimeOffset? LastActionUtc,
    string? LastAction,
    string? LastError,
    int ActionsCount)
{
    public static BotActorSnapshot From(BotActorState actor) =>
        new(
            actor.Id,
            actor.Name,
            actor.Login,
            actor.Role.ToString(),
            actor.User?.WarehouseName ?? actor.PreferredWarehouseId?.ToString(),
            !string.IsNullOrWhiteSpace(actor.Token) && actor.LastError is null,
            actor.NextActionsUtc.OrderBy(x => x.Value).FirstOrDefault().Value == default
                ? null
                : actor.NextActionsUtc.OrderBy(x => x.Value).First().Value,
            actor.NextActionsUtc.OrderBy(x => x.Value).FirstOrDefault().Key,
            actor.LastLoginUtc,
            actor.LastActionUtc,
            actor.LastAction,
            actor.LastError,
            actor.ActionsCount);
}
