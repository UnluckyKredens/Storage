namespace MagazineWarehouseBot.Models;

public sealed class MagazineApiOptions
{
    public const string SectionName = "MagazineApi";

    public string BaseUrl { get; set; } = "http://localhost:5292/api/";

    public string NormalizedBaseUrl => BaseUrl.EndsWith("/", StringComparison.Ordinal)
        ? BaseUrl
        : $"{BaseUrl}/";
}

public sealed class WarehouseBotOptions
{
    public const string SectionName = "WarehouseBot";

    public bool AutoStart { get; set; }
    public bool AutoProvisionTeam { get; set; } = true;
    public int WorkersPerWarehouse { get; set; } = 2;
    public string TeamPassword { get; set; } = "Test123!";
    public int PollSeconds { get; set; } = 5;
    public int MaxRecentEvents { get; set; } = 250;
    public ActionCadenceOptions Cadence { get; set; } = new();
    public List<BotAccountOptions> Accounts { get; set; } = [];
}

public sealed class ActionCadenceOptions
{
    public int InitialDelaySeconds { get; set; } = 15;
    public int WarehouseOperationSeconds { get; set; } = 150;
    public int InventoryChangeSeconds { get; set; } = 120;
    public int ShipmentRequestSeconds { get; set; } = 300;
    public int ShipmentCreateSeconds { get; set; } = 420;
    public int ShipmentApproveSeconds { get; set; } = 180;
    public int ShipmentReceiveSeconds { get; set; } = 240;
    public int PurchaseOrderCreateSeconds { get; set; } = 360;
    public int PurchaseOrderApproveSeconds { get; set; } = 210;
    public int PurchaseOrderReceiveSeconds { get; set; } = 260;
    public int ProductCreateSeconds { get; set; } = 600;
    public int CleanupSeconds { get; set; } = 900;
    public int JitterPercent { get; set; } = 25;
}

public sealed class BotAccountOptions
{
    public string Name { get; set; } = string.Empty;
    public BotRole Role { get; set; } = BotRole.Worker;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid? WarehouseId { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
}

public enum BotRole
{
    Administrator,
    Admin = Administrator,
    Worker,
    Manager
}

public sealed record ConfigureAccountsRequest(IReadOnlyList<BotAccountOptions> Accounts);
