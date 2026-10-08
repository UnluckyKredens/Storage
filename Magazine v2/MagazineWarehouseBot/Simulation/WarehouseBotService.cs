using MagazineWarehouseBot.Api;
using MagazineWarehouseBot.Models;
using Microsoft.Extensions.Options;

namespace MagazineWarehouseBot.Simulation;

public sealed class WarehouseBotService : BackgroundService
{
    private const string ShipmentsCreate = "shipments.create";
    private const string ShipmentsApprove = "shipments.approve";
    private const string ProductsManage = "products.manage";
    private const string InventoryManage = "inventory.manage";
    private const string PurchaseOrdersCreate = "purchase-orders.create";
    private const string PurchaseOrdersApprove = "purchase-orders.approve";

    private readonly BotControlState _state;
    private readonly BotActionLog _log;
    private readonly MagazineApiClient _api;
    private readonly IOptions<WarehouseBotOptions> _options;
    private readonly Dictionary<Guid, Task> _actorTasks = [];
    private readonly Dictionary<BotRole, IReadOnlyList<ScheduledBotAction>> _actionsByRole;
    private DateTimeOffset _nextProvisionAttemptUtc = DateTimeOffset.MinValue;
    private bool _teamProvisioned;

    public WarehouseBotService(
        BotControlState state,
        BotActionLog log,
        MagazineApiClient api,
        IOptions<WarehouseBotOptions> options)
    {
        _state = state;
        _log = log;
        _api = api;
        _options = options;
        var cadence = _options.Value.Cadence;
        _actionsByRole = new Dictionary<BotRole, IReadOnlyList<ScheduledBotAction>>
        {
            [BotRole.Administrator] =
            [
                new("Operacja magazynowa", cadence.WarehouseOperationSeconds, TryCompleteWarehouseOperationAsync),
                new("Akceptacja", cadence.ShipmentApproveSeconds, TryApproveShipmentAsync),
                new("Transport", cadence.ShipmentReceiveSeconds, TryMoveShipmentForwardAsync),
                new("Odbior", cadence.ShipmentReceiveSeconds, TryReceiveShipmentAsync),
                new("Zamowienie", cadence.PurchaseOrderCreateSeconds, TryCreatePurchaseOrderAsync),
                new("Akceptacja zamowienia", cadence.PurchaseOrderApproveSeconds, TryApprovePurchaseOrderAsync),
                new("Przyjecie zamowienia", cadence.PurchaseOrderReceiveSeconds, TryReceivePurchaseOrderAsync),
                new("Stan", cadence.InventoryChangeSeconds, TryTouchInventoryAsync),
                new("Nowy towar", cadence.ProductCreateSeconds, TryCreateProductWithInventoryAsync),
                new("Wysylka", cadence.ShipmentCreateSeconds, TryCreateShipmentAsync),
                new("Prosba o wysylke", cadence.ShipmentRequestSeconds, TryCreateShipmentRequestAsync),
                new("Usuniecie towaru", cadence.CleanupSeconds, TryCleanupBotRecordAsync)
            ],
            [BotRole.Worker] =
            [
                new("Prosba o wysylke", cadence.ShipmentRequestSeconds, TryCreateShipmentRequestAsync),
                new("Wysylka", cadence.ShipmentCreateSeconds, TryCreateShipmentAsync),
                new("Zamowienie", cadence.PurchaseOrderCreateSeconds, TryCreatePurchaseOrderAsync)
            ],
            [BotRole.Manager] =
            [
                new("Operacja magazynowa", cadence.WarehouseOperationSeconds, TryCompleteWarehouseOperationAsync),
                new("Akceptacja", cadence.ShipmentApproveSeconds, TryApproveShipmentAsync),
                new("Transport", cadence.ShipmentReceiveSeconds, TryMoveShipmentForwardAsync),
                new("Odbior", cadence.ShipmentReceiveSeconds, TryReceiveShipmentAsync),
                new("Zamowienie", cadence.PurchaseOrderCreateSeconds, TryCreatePurchaseOrderAsync),
                new("Akceptacja zamowienia", cadence.PurchaseOrderApproveSeconds, TryApprovePurchaseOrderAsync),
                new("Przyjecie zamowienia", cadence.PurchaseOrderReceiveSeconds, TryReceivePurchaseOrderAsync),
                new("Stan", cadence.InventoryChangeSeconds, TryTouchInventoryAsync),
                new("Nowy towar", cadence.ProductCreateSeconds, TryCreateProductWithInventoryAsync),
                new("Wysylka", cadence.ShipmentCreateSeconds, TryCreateShipmentAsync),
                new("Usuniecie towaru", cadence.CleanupSeconds, TryCleanupBotRecordAsync)
            ]
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.Info("System", "Gotowy", "Bot czeka na konta i start symulacji.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_state.IsRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            if (_options.Value.AutoProvisionTeam)
            {
                await TryEnsureDemoTeamAsync(stoppingToken);
            }

            foreach (var actor in _state.GetActors())
            {
                if (_actorTasks.TryGetValue(actor.Id, out var task) && !task.IsCompleted)
                {
                    continue;
                }

                _actorTasks[actor.Id] = RunActorAsync(actor.Id, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task RunActorAsync(Guid actorId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested &&
               _state.IsRunning &&
               _state.TryGetActor(actorId, out var actor) &&
               actor is not null)
        {
            try
            {
                await EnsureSessionAsync(actor, cancellationToken);
                EnsureSchedule(actor);
                await DoWorkAsync(actor, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _state.MarkAction(actor, "Blad", exception.Message);
                _log.Warn(actor.Name, "Blad", exception.Message);
            }

            await Task.Delay(PollDelay(), cancellationToken);
        }
    }

    private async Task EnsureSessionAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(actor.Token) &&
            actor.LastLoginUtc is not null &&
            DateTimeOffset.UtcNow - actor.LastLoginUtc < TimeSpan.FromMinutes(45))
        {
            return;
        }

        var login = await _api.LoginAsync(actor.Login, actor.Password, cancellationToken);
        var user = await _api.MeAsync(login.Token, cancellationToken);
        var permissions = await _api.PermissionsAsync(login.Token, cancellationToken);
        _state.MarkLogin(actor, login.Token, user, permissions);
        _log.Info(actor.Name, "Logowanie", $"Zalogowano jako {user?.FirstName} {user?.LastName} ({user?.RoleName}).");
    }

    private async Task TryEnsureDemoTeamAsync(CancellationToken cancellationToken)
    {
        if (_teamProvisioned || DateTimeOffset.UtcNow < _nextProvisionAttemptUtc)
        {
            return;
        }

        _nextProvisionAttemptUtc = DateTimeOffset.UtcNow.AddSeconds(30);

        var admin = _state.GetActors()
            .FirstOrDefault(actor => actor.Role == BotRole.Administrator);
        if (admin is null)
        {
            _log.Warn("System", "Provisioning", "Brak konta administratora do automatycznego przygotowania zespolu.");
            return;
        }

        try
        {
            await EnsureSessionAsync(admin, cancellationToken);
            if (string.IsNullOrWhiteSpace(admin.Token))
            {
                return;
            }

            var warehouses = await EnsureBaseStructureAsync(admin.Token, cancellationToken);
            var roles = await _api.RolesAsync(admin.Token, cancellationToken);
            var users = await _api.UsersAsync(admin.Token, cancellationToken);
            var managerRole = roles.FirstOrDefault(role => role.Name.Contains("Kierownik", StringComparison.OrdinalIgnoreCase));
            var workerRole = roles.FirstOrDefault(role => role.Name.Contains("Pracownik", StringComparison.OrdinalIgnoreCase));

            if (managerRole is null || workerRole is null || warehouses.Count == 0)
            {
                _log.Warn("System", "Provisioning", "Brak rol albo magazynow wymaganych do utworzenia zespolu.");
                return;
            }

            var generatedAccounts = new List<BotAccountOptions>();
            foreach (var warehouse in warehouses)
            {
                var code = BranchCode(warehouse.Name);
                generatedAccounts.Add(new BotAccountOptions
                {
                    Name = $"Kierownik {ShortWarehouseName(warehouse.Name)}",
                    Login = $"bot.kierownik.{code}",
                    Password = _options.Value.TeamPassword,
                    Role = BotRole.Manager,
                    WarehouseId = warehouse.Id
                });

                for (var index = 1; index <= Math.Max(_options.Value.WorkersPerWarehouse, 0); index++)
                {
                    generatedAccounts.Add(new BotAccountOptions
                    {
                        Name = $"Pracownik {ShortWarehouseName(warehouse.Name)} {index}",
                        Login = $"bot.pracownik.{code}.{index}",
                        Password = _options.Value.TeamPassword,
                        Role = BotRole.Worker,
                        WarehouseId = warehouse.Id
                    });
                }
            }

            var existingLogins = users.Select(user => user.Login).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var account in generatedAccounts.Where(account => !existingLogins.Contains(account.Login)))
            {
                var role = account.Role == BotRole.Manager ? managerRole : workerRole;
                await _api.CreateUserAsync(
                    admin.Token,
                    new UserSaveRequest(
                        account.Login,
                        FirstName(account),
                        LastName(account),
                        $"{account.Login}@bot.magazine.local",
                        account.Password,
                        role.Id,
                        account.WarehouseId),
                    cancellationToken);
                _log.Info("Administrator", "Provisioning", $"Utworzono konto {account.Login}.");
            }

            _state.EnsureAccounts(generatedAccounts);
            _teamProvisioned = true;
            _log.Info("System", "Provisioning", $"Zespol botow gotowy: {generatedAccounts.Count} kont dla {warehouses.Count} oddzialow.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _log.Warn("System", "Provisioning", exception.Message);
        }
    }

    private async Task<IReadOnlyList<WarehouseView>> EnsureBaseStructureAsync(
        string adminToken,
        CancellationToken cancellationToken)
    {
        var dictionaries = await EnsureDictionariesAsync(adminToken, cancellationToken);

        var warehouses = (await _api.WarehousesAsync(adminToken, cancellationToken)).ToList();
        if (warehouses.Count == 0)
        {
            foreach (var branch in DefaultBranches)
            {
                var warehouse = await _api.CreateWarehouseAsync(
                    adminToken,
                    new WarehouseSaveRequest(null, branch.Name, branch.Address, branch.Description),
                    cancellationToken);
                if (warehouse is not null)
                {
                    warehouses.Add(warehouse);
                    _log.Info("Administrator", "Bootstrap", $"Utworzono magazyn {warehouse.Name}.");
                }
            }
        }

        var locations = await _api.LocationsAsync(adminToken, cancellationToken);
        var existing = locations
            .Select(location => (location.WarehouseId, Code: location.LocationCode))
            .ToHashSet();

        foreach (var warehouse in warehouses)
        {
            foreach (var location in DefaultLocations)
            {
                if (existing.Contains((warehouse.Id, location.Code)))
                {
                    continue;
                }

                await _api.CreateLocationAsync(
                    adminToken,
                    new LocationSaveRequest(null, warehouse.Id, location.Code, location.Description),
                    cancellationToken);
                _log.Info("Administrator", "Bootstrap", $"Utworzono lokalizacje {location.Code} w {warehouse.Name}.");
            }
        }

        locations = await _api.LocationsAsync(adminToken, cancellationToken);
        await EnsureInitialStockAsync(adminToken, warehouses, locations, dictionaries, cancellationToken);
        await EnsureSuppliersAsync(adminToken, cancellationToken);

        return warehouses;
    }

    private async Task<BootstrapDictionaries> EnsureDictionariesAsync(string adminToken, CancellationToken cancellationToken)
    {
        var categories = await _api.CategoriesAsync(adminToken, cancellationToken);
        var category = categories.FirstOrDefault(category =>
            category.Name.Equals("Sprzet IT", StringComparison.OrdinalIgnoreCase));
        if (category is null)
        {
            category = await _api.CreateCategoryAsync(
                adminToken,
                new CategorySaveRequest(null, "Sprzet IT", "Kategoria utworzona automatycznie przez bota."),
                cancellationToken);
            _log.Info("Administrator", "Bootstrap", "Utworzono kategorie Sprzet IT.");
        }

        var units = await _api.UnitsAsync(adminToken, cancellationToken);
        var unit = units.FirstOrDefault(unit =>
            unit.Symbol.Equals("szt.", StringComparison.OrdinalIgnoreCase));
        if (unit is null)
        {
            unit = await _api.CreateUnitAsync(
                adminToken,
                new UnitOfMeasureSaveRequest(null, "Sztuka", "szt."),
                cancellationToken);
            _log.Info("Administrator", "Bootstrap", "Utworzono jednostke szt.");
        }

        category ??= (await _api.CategoriesAsync(adminToken, cancellationToken))
            .First(category => category.Name.Equals("Sprzet IT", StringComparison.OrdinalIgnoreCase));
        unit ??= (await _api.UnitsAsync(adminToken, cancellationToken))
            .First(unit => unit.Symbol.Equals("szt.", StringComparison.OrdinalIgnoreCase));

        return new BootstrapDictionaries(category, unit);
    }

    private async Task EnsureInitialStockAsync(
        string adminToken,
        IReadOnlyList<WarehouseView> warehouses,
        IReadOnlyList<LocationView> locations,
        BootstrapDictionaries dictionaries,
        CancellationToken cancellationToken)
    {
        var products = (await _api.ProductsAsync(adminToken, cancellationToken)).List;
        var productsBySku = products.ToDictionary(product => product.Sku, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in DefaultProducts)
        {
            if (productsBySku.ContainsKey(seed.Sku))
            {
                continue;
            }

            var created = await _api.CreateProductAsync(
                adminToken,
                new ProductSaveRequest(
                    seed.Name,
                    seed.Sku,
                    seed.Barcode,
                    "Produkt startowy do symulacji pracy magazynu.",
                    seed.ImageUrl,
                    dictionaries.Unit.Id,
                    dictionaries.Category.Id,
                    seed.PurchasePrice,
                    seed.SalePrice,
                    true),
                cancellationToken);

            if (created is not null)
            {
                productsBySku[created.Sku] = created;
                _log.Info("Administrator", "Bootstrap", $"Utworzono produkt startowy {created.Sku}.");
            }
        }

        var seededProducts = DefaultProducts
            .Select(seed => productsBySku.GetValueOrDefault(seed.Sku))
            .OfType<ProductReadModel>()
            .ToArray();
        if (seededProducts.Length == 0)
        {
            return;
        }

        foreach (var warehouse in warehouses)
        {
            var warehouseLocations = locations
                .Where(location => location.WarehouseId == warehouse.Id)
                .OrderBy(location => LocationPriority(location.LocationCode))
                .ThenBy(location => location.LocationCode)
                .Take(3)
                .ToArray();
            if (warehouseLocations.Length == 0)
            {
                continue;
            }

            var inventory = await _api.InventoryAsync(adminToken, warehouse.Id, cancellationToken);
            var existingPairs = inventory
                .Select(item => (item.ProductId, item.LocationId))
                .ToHashSet();
            var createdCount = 0;

            for (var index = 0; index < seededProducts.Length; index++)
            {
                var product = seededProducts[index];
                var location = warehouseLocations[index % warehouseLocations.Length];
                if (existingPairs.Contains((product.ProductId, location.Id)))
                {
                    continue;
                }

                await _api.CreateInventoryAsync(
                    adminToken,
                    new InventorySaveRequest(
                        null,
                        product.ProductId,
                        location.Id,
                        Random.Shared.Next(12, 48),
                        0),
                    warehouse.Id,
                    cancellationToken);
                createdCount++;
            }

            if (createdCount > 0)
            {
                _log.Info("Administrator", "Bootstrap", $"Zasilono {warehouse.Name}: {createdCount} pozycji startowych.");
            }
        }
    }

    private async Task EnsureSuppliersAsync(string adminToken, CancellationToken cancellationToken)
    {
        var contractors = await _api.ContractorsAsync(adminToken, cancellationToken);
        if (contractors.Any(contractor => contractor.Type is 1 or 3))
        {
            return;
        }

        foreach (var supplier in DefaultSuppliers)
        {
            await _api.CreateContractorAsync(
                adminToken,
                new ContractorSaveRequest(
                    null,
                    supplier.Name,
                    supplier.TaxNumber,
                    1,
                    supplier.Email,
                    supplier.Phone,
                    supplier.Address),
                cancellationToken);
            _log.Info("Administrator", "Bootstrap", $"Utworzono dostawce {supplier.Name}.");
        }
    }

    private async Task DoWorkAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor.Token))
        {
            return;
        }

        if (!_actionsByRole.TryGetValue(actor.Role, out var actions))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var dueAction = actions
            .Where(action => actor.NextActionsUtc.TryGetValue(action.Name, out var dueAt) && dueAt <= now)
            .OrderBy(action => actor.NextActionsUtc[action.Name])
            .ThenBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (dueAction is null)
        {
            return;
        }

        ScheduleNext(actor, dueAction);

        var success = await dueAction.Work(actor, cancellationToken);
        if (!success)
        {
            _state.MarkWaiting(actor, $"Pominieto: {dueAction.Name}", "Brak danych albo uprawnien dla tej akcji.");
        }
    }

    private async Task<bool> TryCreateShipmentRequestAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(ShipmentsCreate))
        {
            return false;
        }

        var warehouseId = await ResolveWarehouseContextAsync(actor, cancellationToken);
        if (warehouseId is null)
        {
            return false;
        }

        var products = (await _api.ProductsAsync(actor.Token!, cancellationToken)).List
            .Where(x => x.IsActive)
            .OrderBy(_ => Random.Shared.Next())
            .Take(Random.Shared.Next(1, 4))
            .Select(x => new CreateShipmentItem(null, x.ProductId, Random.Shared.Next(1, 5)))
            .ToArray();

        if (products.Length == 0)
        {
            return false;
        }

        var shipment = await _api.CreateShipmentRequestAsync(
            actor.Token!,
            new CreateShipmentDemandRequest(products),
            warehouseId,
            cancellationToken);

        return Done(actor, "Prosba o wysylke", $"Utworzono zapotrzebowanie {shipment?.Number} ({products.Length} pozycji).");
    }

    private async Task<bool> TryCreateShipmentAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(ShipmentsCreate))
        {
            return false;
        }

        var warehouseId = await ResolveWarehouseContextAsync(actor, cancellationToken);
        if (warehouseId is null)
        {
            return false;
        }

        var pageData = await _api.ShipmentPageDataAsync(actor.Token!, warehouseId, cancellationToken);
        if (pageData is null || pageData.DestinationWarehouses.Count == 0)
        {
            return false;
        }

        var inventory = await _api.InventoryAsync(actor.Token!, warehouseId, cancellationToken);
        var available = inventory
            .Where(x => x.WarehouseId == pageData.SourceWarehouse.Id && x.AvailableQuantity > 0)
            .GroupBy(x => x.ProductId)
            .Select(x => new
            {
                ProductId = x.Key,
                Quantity = x.Sum(i => i.AvailableQuantity)
            })
            .Where(x => x.Quantity > 0)
            .OrderBy(_ => Random.Shared.Next())
            .Take(Random.Shared.Next(1, 4))
            .ToArray();

        if (available.Length == 0)
        {
            return false;
        }

        var destination = pageData.DestinationWarehouses[Random.Shared.Next(pageData.DestinationWarehouses.Count)];
        var items = available
            .Select(x => new CreateShipmentItem(null, x.ProductId, Math.Min(x.Quantity, Random.Shared.Next(1, 4))))
            .ToArray();

        var shipment = await _api.CreateShipmentAsync(
            actor.Token!,
            new CreateShipmentRequest(destination.Id, items),
            warehouseId,
            cancellationToken);

        return Done(actor, "Wysylka", $"Utworzono wysylke {shipment?.Number} do {destination.Name}.");
    }

    private async Task<bool> TryApproveShipmentAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(ShipmentsApprove))
        {
            return false;
        }

        var shipment = (await _api.ShipmentsAsync(actor.Token!, cancellationToken))
            .Where(x => x.Status.Contains("Oczekuje", StringComparison.OrdinalIgnoreCase))
            .Where(x => actor.User?.WarehouseId is null || x.SourceWarehouseId == actor.User.WarehouseId)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (shipment is null)
        {
            return false;
        }

        await _api.ApproveShipmentAsync(actor.Token!, shipment.Id, cancellationToken);
        return Done(actor, "Akceptacja", $"Zaakceptowano {shipment.Number} ({shipment.SourceWarehouseName} -> {shipment.DestinationWarehouseName}).");
    }

    private async Task<bool> TryReceiveShipmentAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(ShipmentsApprove))
        {
            return false;
        }

        var shipment = (await _api.ShipmentsAsync(actor.Token!, cancellationToken))
            .Where(x => x.Status.Contains("W drodze", StringComparison.OrdinalIgnoreCase))
            .Where(x => actor.User?.WarehouseId is null || x.DestinationWarehouseId == actor.User.WarehouseId)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (shipment is null)
        {
            return false;
        }

        await _api.ReceiveShipmentAsync(
            actor.Token!,
            shipment.Id,
            new ReceiveShipmentRequest(
                shipment.Items.Select(item => item.Id).ToArray(),
                "Odbior potwierdzony automatycznie przez bota."),
            cancellationToken);
        return Done(actor, "Odbior", $"Przyjeto wysylke {shipment.Number} w {shipment.DestinationWarehouseName}.");
    }

    private async Task<bool> TryMoveShipmentForwardAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(ShipmentsApprove))
        {
            return false;
        }

        var shipment = (await _api.ShipmentsAsync(actor.Token!, cancellationToken))
            .Where(x => x.Status.Equals("Wysłana", StringComparison.OrdinalIgnoreCase))
            .Where(x => actor.User?.WarehouseId is null || x.SourceWarehouseId == actor.User.WarehouseId)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (shipment is null)
        {
            return false;
        }

        await _api.MarkShipmentInTransitAsync(actor.Token!, shipment.Id, cancellationToken);
        return Done(actor, "Transport", $"Oznaczono {shipment.Number} jako w drodze.");
    }

    private async Task<bool> TryCreatePurchaseOrderAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(PurchaseOrdersCreate))
        {
            return false;
        }

        var warehouseId = await ResolveWarehouseContextAsync(actor, cancellationToken);
        if (warehouseId is null)
        {
            return false;
        }

        var pageData = await _api.PurchaseOrderPageDataAsync(actor.Token!, warehouseId, cancellationToken);
        if (pageData is null || pageData.Suppliers.Count == 0 || pageData.Products.Count == 0)
        {
            return false;
        }

        var supplier = pageData.Suppliers[Random.Shared.Next(pageData.Suppliers.Count)];
        var items = pageData.Products
            .OrderBy(_ => Random.Shared.Next())
            .Take(Random.Shared.Next(1, Math.Min(4, pageData.Products.Count + 1)))
            .Select(product => new CreatePurchaseOrderItem(
                product.ProductId,
                Random.Shared.Next(2, 12),
                Math.Round(product.PurchasePrice * (decimal)(0.95 + Random.Shared.NextDouble() * 0.12), 2)))
            .ToArray();

        var order = await _api.CreatePurchaseOrderAsync(
            actor.Token!,
            new CreatePurchaseOrderRequest(
                supplier.Id,
                items,
                "Zamowienie utworzone automatycznie przez bota."),
            warehouseId,
            cancellationToken);

        return Done(actor, "Zamowienie", $"Utworzono zamowienie {order?.Number} od {supplier.Name}.");
    }

    private async Task<bool> TryApprovePurchaseOrderAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(PurchaseOrdersApprove))
        {
            return false;
        }

        var order = (await _api.PurchaseOrdersAsync(actor.Token!, cancellationToken))
            .Where(x => x.Status.Contains("Oczekuje", StringComparison.OrdinalIgnoreCase))
            .Where(x => actor.User?.WarehouseId is null || x.WarehouseId == actor.User.WarehouseId)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (order is null)
        {
            return false;
        }

        var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        await _api.ApprovePurchaseOrderAsync(
            actor.Token!,
            order.Id,
            new ApprovePurchaseOrderRequest(
                $"FV/BOT/{suffix}",
                $"PZ-BOT/{suffix}",
                "Faktura i dokument papierowy zaakceptowane automatycznie."),
            cancellationToken);

        return Done(actor, "Akceptacja zamowienia", $"Zaakceptowano zamowienie {order.Number}.");
    }

    private async Task<bool> TryReceivePurchaseOrderAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(PurchaseOrdersApprove))
        {
            return false;
        }

        var order = (await _api.PurchaseOrdersAsync(actor.Token!, cancellationToken))
            .Where(x => x.Status.Equals("Zaakceptowane", StringComparison.OrdinalIgnoreCase))
            .Where(x => actor.User?.WarehouseId is null || x.WarehouseId == actor.User.WarehouseId)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (order is null)
        {
            return false;
        }

        await _api.ReceivePurchaseOrderAsync(
            actor.Token!,
            order.Id,
            new ReceivePurchaseOrderRequest(
                order.Items.Select(item => item.Id).ToArray(),
                "Przyjecie zamowienia potwierdzone automatycznie przez bota."),
            cancellationToken);

        return Done(actor, "Przyjecie zamowienia", $"Przyjeto zamowienie {order.Number} na stan.");
    }

    private async Task<bool> TryTouchInventoryAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(InventoryManage))
        {
            return false;
        }

        var warehouseId = await ResolveWarehouseContextAsync(actor, cancellationToken);
        var inventory = await _api.InventoryAsync(actor.Token!, warehouseId, cancellationToken);
        var item = inventory
            .Where(x => x.Quantity > 0)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (item is null)
        {
            return false;
        }

        var delta = Random.Shared.Next(-3, 8);
        var newQuantity = Math.Max(0, item.Quantity + delta);
        var newReserved = Math.Min(item.ReservedQuantity, newQuantity);
        await _api.UpdateInventoryAsync(
            actor.Token!,
            item.Id,
            new InventorySaveRequest(item.Id, item.ProductId, item.LocationId, newQuantity, newReserved),
            warehouseId,
            cancellationToken);

        return Done(actor, "Stan", $"{item.ProductName}: {item.Quantity} -> {newQuantity} w {item.LocationCode}.");
    }

    private async Task<bool> TryCreateProductWithInventoryAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(ProductsManage) || !actor.HasPermission(InventoryManage))
        {
            return false;
        }

        var productPageData = await _api.ProductPageDataAsync(actor.Token!, cancellationToken);
        var warehouseId = await ResolveWarehouseContextAsync(actor, cancellationToken);
        var inventoryPageData = await _api.InventoryPageDataAsync(actor.Token!, warehouseId, cancellationToken);
        var category = productPageData?.Categories.OrderBy(_ => Random.Shared.Next()).FirstOrDefault();
        var unit = productPageData?.Units.OrderBy(_ => Random.Shared.Next()).FirstOrDefault();
        var location = inventoryPageData?.Locations.OrderBy(_ => Random.Shared.Next()).FirstOrDefault();

        if (category is null || unit is null || location is null)
        {
            return false;
        }

        var stamp = DateTime.UtcNow.ToString("MMddHHmmssfff");
        var product = await _api.CreateProductAsync(
            actor.Token!,
            new ProductSaveRequest(
                $"BOT {HardwareNames[Random.Shared.Next(HardwareNames.Length)]} {stamp}",
                $"BOT-{stamp}",
                $"29{Random.Shared.NextInt64(10000000000, 99999999999)}",
                "Rekord wygenerowany przez symulator magazynu.",
                null,
                unit.Id,
                category.Id,
                Random.Shared.Next(30, 900),
                Random.Shared.Next(100, 1800),
                true),
            cancellationToken);

        if (product is null)
        {
            return false;
        }

        await _api.CreateInventoryAsync(
            actor.Token!,
            new InventorySaveRequest(null, product.ProductId, location.Id, Random.Shared.Next(2, 30), 0),
            location.WarehouseId,
            cancellationToken);

        return Done(actor, "Nowy towar", $"Dodano {product.Name} i stan w {location.Code}.");
    }

    private async Task<bool> TryCompleteWarehouseOperationAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(InventoryManage))
        {
            return false;
        }

        var warehouseId = await ResolveWarehouseContextAsync(actor, cancellationToken);
        if (warehouseId is null)
        {
            return false;
        }

        var inventoryPageData = await _api.InventoryPageDataAsync(actor.Token!, warehouseId, cancellationToken);
        if (inventoryPageData is null || inventoryPageData.Products.Count == 0 || inventoryPageData.Locations.Count == 0)
        {
            return false;
        }

        var inventory = await _api.InventoryAsync(actor.Token!, warehouseId, cancellationToken);
        var operationType = Random.Shared.Next(1, 6);
        var request = BuildWarehouseOperation(operationType, warehouseId.Value, inventoryPageData, inventory);
        if (request is null)
        {
            return false;
        }

        var result = await _api.CompleteWarehouseOperationAsync(actor.Token!, request, warehouseId, cancellationToken);
        return Done(actor, "Operacja magazynowa", $"Zaksiegowano dokument typu {request.Type} ({result?.Id}).");
    }

    private async Task<bool> TryCleanupBotRecordAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (!actor.HasPermission(InventoryManage))
        {
            return false;
        }

        var warehouseId = await ResolveWarehouseContextAsync(actor, cancellationToken);
        var inventory = await _api.InventoryAsync(actor.Token!, warehouseId, cancellationToken);
        var botInventory = inventory
            .Where(x => x.ProductName.StartsWith("BOT ", StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (botInventory is not null)
        {
            await _api.DeleteInventoryAsync(actor.Token!, botInventory.Id, warehouseId, cancellationToken);
            Done(actor, "Usuniecie stanu", $"Usunieto stan {botInventory.ProductName} z {botInventory.LocationCode}.");
            return true;
        }

        if (!actor.HasPermission(ProductsManage))
        {
            return false;
        }

        var product = (await _api.ProductsAsync(actor.Token!, cancellationToken)).List
            .Where(x => x.Sku.StartsWith("BOT-", StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();

        if (product is null)
        {
            return false;
        }

        await _api.DeleteProductAsync(actor.Token!, product.ProductId, cancellationToken);
        return Done(actor, "Usuniecie towaru", $"Usunieto produkt {product.Name}.");
    }

    private async Task<Guid?> ResolveWarehouseContextAsync(BotActorState actor, CancellationToken cancellationToken)
    {
        if (actor.User?.WarehouseId is not null)
        {
            return actor.User.WarehouseId;
        }

        if (actor.PreferredWarehouseId is not null)
        {
            return actor.PreferredWarehouseId;
        }

        var warehouses = await _api.WarehousesAsync(actor.Token!, cancellationToken);
        return warehouses.OrderBy(_ => Random.Shared.Next()).FirstOrDefault()?.Id;
    }

    private bool Done(BotActorState actor, string action, string message)
    {
        _state.MarkAction(actor, action);
        _log.Info(actor.Name, action, message);
        return true;
    }

    private void EnsureSchedule(BotActorState actor)
    {
        if (actor.ScheduleInitialized ||
            !_actionsByRole.TryGetValue(actor.Role, out var actions))
        {
            return;
        }

        foreach (var action in actions)
        {
            ScheduleInitial(actor, action);
        }

        actor.ScheduleInitialized = true;
        var next = actor.NextActionsUtc.OrderBy(x => x.Value).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(next.Key))
        {
            _log.Info(actor.Name, "Harmonogram", $"Najblizsza akcja: {next.Key} o {next.Value.LocalDateTime:HH:mm:ss}.");
        }
    }

    private void ScheduleNext(BotActorState actor, ScheduledBotAction action)
    {
        var delay = RandomizedInterval(action.IntervalSeconds);
        _state.ScheduleAction(actor, action.Name, DateTimeOffset.UtcNow.Add(delay));
    }

    private void ScheduleInitial(BotActorState actor, ScheduledBotAction action)
    {
        var maxDelay = Math.Max(_options.Value.Cadence.InitialDelaySeconds, 1);
        var delay = TimeSpan.FromSeconds(Random.Shared.Next(1, maxDelay + 1));
        _state.ScheduleAction(actor, action.Name, DateTimeOffset.UtcNow.Add(delay));
    }

    private static int LocationPriority(string code) => code switch
    {
        "A-01" => 0,
        "B-01" => 1,
        "PICK-01" => 2,
        "REC-01" => 3,
        _ => 4
    };

    private TimeSpan RandomizedInterval(int seconds)
    {
        var cadence = _options.Value.Cadence;
        var jitterPercent = Math.Clamp(cadence.JitterPercent, 0, 90);
        var minimum = Math.Max(10, seconds * (100 - jitterPercent) / 100);
        var maximum = Math.Max(minimum + 1, seconds * (100 + jitterPercent) / 100);
        return TimeSpan.FromSeconds(Random.Shared.Next(minimum, maximum + 1));
    }

    private TimeSpan PollDelay()
    {
        return TimeSpan.FromSeconds(Math.Max(_options.Value.PollSeconds, 1));
    }

    private static readonly string[] HardwareNames =
    [
        "SSD NVMe 2TB",
        "Router WiFi 7",
        "Switch 24p PoE",
        "Zasilacz ATX 850W",
        "Pamiec DDR5 32GB",
        "Karta graficzna RTX",
        "Monitor 27 IPS",
        "Obudowa rack",
        "Klawiatura mechaniczna",
        "Dock USB-C"
    ];

    private static readonly ProductSeed[] DefaultProducts =
    [
        new("Laptop biznesowy 14", "BOT-DEMO-LAP-14", "5900000000011", "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=900&q=80", 2450, 3290),
        new("Monitor 27 IPS", "BOT-DEMO-MON-27", "5900000000028", "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=900&q=80", 620, 949),
        new("Stacja dokujaca USB-C", "BOT-DEMO-DOCK", "5900000000035", "https://images.unsplash.com/photo-1625842268584-8f3296236761?auto=format&fit=crop&w=900&q=80", 310, 529),
        new("Dysk SSD NVMe 2TB", "BOT-DEMO-SSD-2TB", "5900000000042", "https://images.unsplash.com/photo-1597138804456-e7dca7f59d36?auto=format&fit=crop&w=900&q=80", 390, 649),
        new("Switch 24p PoE", "BOT-DEMO-SW-24P", "5900000000059", "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=900&q=80", 980, 1490),
        new("Router WiFi 7", "BOT-DEMO-RT-WIFI7", "5900000000066", "https://images.unsplash.com/photo-1606904825846-647eb07f5be2?auto=format&fit=crop&w=900&q=80", 520, 849)
    ];

    private static readonly SupplierSeed[] DefaultSuppliers =
    [
        new("Bot Supply Europe sp. z o.o.", "5273011122", "zakupy@botsupply.local", "+48 22 100 20 30", "ul. Dostawcza 8, 05-500 Piaseczno"),
        new("Automation Hardware S.A.", "7793004455", "orders@automationhardware.local", "+48 61 222 10 10", "ul. Technologiczna 14, 60-179 Poznan")
    ];

    private static readonly BranchSeed[] DefaultBranches =
    [
        new("Warszawa - Centrum Dystrybucyjne", "ul. Logistyczna 1, 05-090 Sekocin Stary", "Oddzial centralny utworzony przez bota."),
        new("Poznan - Oddzial Zachod", "ul. Magazynowa 24, 62-080 Tarnowo Podgorne", "Oddzial zachodni utworzony przez bota."),
        new("Krakow - Oddzial Poludnie", "ul. Przemyslowa 42, 32-085 Modlniczka", "Oddzial poludniowy utworzony przez bota."),
        new("Gdansk - Oddzial Polnoc", "ul. Kontenerowa 19, 80-601 Gdansk", "Oddzial polnocny utworzony przez bota.")
    ];

    private static readonly LocationSeed[] DefaultLocations =
    [
        new("REC-01", "Przyjecia i kontrola dostaw"),
        new("A-01", "Regaly glowne"),
        new("B-01", "Podzespoly i siec"),
        new("PICK-01", "Kompletacja"),
        new("RET-01", "Zwroty i reklamacje")
    ];

    private static WarehouseOperationRequest? BuildWarehouseOperation(
        int operationType,
        Guid warehouseId,
        InventoryPageDataView pageData,
        IReadOnlyList<InventoryView> inventory)
    {
        var locations = pageData.Locations
            .Where(location => location.WarehouseId == warehouseId)
            .ToArray();
        if (locations.Length == 0)
        {
            return null;
        }

        var availableInventory = inventory
            .Where(item => item.WarehouseId == warehouseId && item.AvailableQuantity > 0)
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();

        return operationType switch
        {
            1 => BuildReceipt(warehouseId, pageData, locations),
            2 => BuildIssue(warehouseId, availableInventory),
            3 => BuildTransfer(warehouseId, locations, availableInventory),
            4 => BuildTargetQuantity(warehouseId, availableInventory, false),
            5 => BuildTargetQuantity(warehouseId, availableInventory, true),
            _ => null
        };
    }

    private static WarehouseOperationRequest? BuildReceipt(
        Guid warehouseId,
        InventoryPageDataView pageData,
        IReadOnlyList<LocationOption> locations)
    {
        var product = pageData.Products.OrderBy(_ => Random.Shared.Next()).FirstOrDefault();
        var destination = locations.OrderBy(_ => Random.Shared.Next()).FirstOrDefault();
        if (product is null || destination is null)
        {
            return null;
        }

        return new WarehouseOperationRequest(
            1,
            warehouseId,
            "Automatyczne przyjecie wewnetrzne.",
            [new WarehouseOperationItemRequest(product.Id, null, destination.Id, Random.Shared.Next(1, 12), null)]);
    }

    private static WarehouseOperationRequest? BuildIssue(Guid warehouseId, IReadOnlyList<InventoryView> inventory)
    {
        var source = inventory.FirstOrDefault();
        if (source is null)
        {
            return null;
        }

        return new WarehouseOperationRequest(
            2,
            warehouseId,
            "Automatyczny rozchod wewnetrzny.",
            [new WarehouseOperationItemRequest(source.ProductId, source.LocationId, null, Math.Min(source.AvailableQuantity, Random.Shared.Next(1, 4)), null)]);
    }

    private static WarehouseOperationRequest? BuildTransfer(
        Guid warehouseId,
        IReadOnlyList<LocationOption> locations,
        IReadOnlyList<InventoryView> inventory)
    {
        var source = inventory.FirstOrDefault();
        if (source is null)
        {
            return null;
        }

        var destination = locations
            .Where(location => location.Id != source.LocationId)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();
        if (destination is null)
        {
            return null;
        }

        return new WarehouseOperationRequest(
            3,
            warehouseId,
            "Automatyczne przesuniecie MM.",
            [new WarehouseOperationItemRequest(source.ProductId, source.LocationId, destination.Id, Math.Min(source.AvailableQuantity, Random.Shared.Next(1, 5)), null)]);
    }

    private static WarehouseOperationRequest? BuildTargetQuantity(
        Guid warehouseId,
        IReadOnlyList<InventoryView> inventory,
        bool inventoryCount)
    {
        var source = inventory.FirstOrDefault();
        if (source is null)
        {
            return null;
        }

        var targetQuantity = Math.Max(source.ReservedQuantity, source.Quantity + Random.Shared.Next(-3, 8));
        return new WarehouseOperationRequest(
            inventoryCount ? 5 : 4,
            warehouseId,
            inventoryCount ? "Automatyczna inwentaryzacja." : "Automatyczna korekta stanu.",
            [new WarehouseOperationItemRequest(source.ProductId, source.LocationId, null, 0, targetQuantity)]);
    }

    private static string FirstName(BotAccountOptions account) =>
        account.Role == BotRole.Manager ? "Kierownik" : "Pracownik";

    private static string LastName(BotAccountOptions account) =>
        account.Name
            .Replace("Kierownik", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Pracownik", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

    private static string ShortWarehouseName(string warehouseName)
    {
        var name = warehouseName.Split('-', 2)[0].Trim();
        return string.IsNullOrWhiteSpace(name) ? warehouseName : name;
    }

    private static string BranchCode(string warehouseName)
    {
        if (warehouseName.Contains("Warsz", StringComparison.OrdinalIgnoreCase)) return "waw";
        if (warehouseName.Contains("Poz", StringComparison.OrdinalIgnoreCase)) return "poz";
        if (warehouseName.Contains("Krak", StringComparison.OrdinalIgnoreCase)) return "krk";
        if (warehouseName.Contains("Gda", StringComparison.OrdinalIgnoreCase)) return "gdn";

        var code = new string(warehouseName
            .Where(char.IsLetterOrDigit)
            .Take(6)
            .Select(char.ToLowerInvariant)
            .ToArray());
        return string.IsNullOrWhiteSpace(code) ? Guid.NewGuid().ToString("N")[..6] : code;
    }

    private sealed record ScheduledBotAction(
        string Name,
        int IntervalSeconds,
        Func<BotActorState, CancellationToken, Task<bool>> Work);

    private sealed record BootstrapDictionaries(CategoryView Category, UnitOfMeasureView Unit);

    private sealed record ProductSeed(
        string Name,
        string Sku,
        string Barcode,
        string ImageUrl,
        decimal PurchasePrice,
        decimal SalePrice);

    private sealed record SupplierSeed(
        string Name,
        string TaxNumber,
        string Email,
        string Phone,
        string Address);

    private sealed record BranchSeed(string Name, string Address, string Description);

    private sealed record LocationSeed(string Code, string Description);
}
