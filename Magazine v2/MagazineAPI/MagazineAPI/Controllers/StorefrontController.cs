using MagazineAPI.Authorization;
using MagazineAPIApplication.Common;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPInfrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MagazineAPI.Services;
using MagazineAPIDomain.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mediator;
using MagazineAPIEvent.Request;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StorefrontController(
    AppDbContext dbContext,
    IWarehouseContext warehouseContext,
    IDocumentNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    StorefrontOrderService orderService,
    IAuditLogWriter auditLogWriter,
    IPublisher publisher) : ControllerBase
{
    public sealed record StorefrontProductResponse(
        Guid Id,
        string Name,
        string Sku,
        string? Barcode,
        string? Description,
        string? ImageUrl,
        decimal Price,
        decimal AvailableQuantity);

    public sealed record StorefrontOrderRequest(
        string CustomerName,
        string CustomerEmail,
        string? CustomerPhone,
        string? DeliveryAddress,
        string? Notes,
        IReadOnlyList<StorefrontOrderItemRequest> Items,
        string? IdempotencyKey = null);

    public sealed record StorefrontOrderItemRequest(Guid ProductId, decimal Quantity);

    public sealed record StorefrontOrderResponse(
        Guid Id,
        string Number,
        decimal TotalValue,
        Guid AssignedWarehouseId,
        string AssignedWarehouseName,
        string TrackingToken);

    public sealed record StorefrontTrackingResponse(
        Guid Id,
        string Number,
        string Status,
        string CurrentLocation,
        string Description,
        DateTime CreatedOnUtc,
        decimal TotalValue,
        IReadOnlyList<StorefrontTrackingItemResponse> Items);

    public sealed record StorefrontTrackingItemResponse(
        string ProductName,
        string Sku,
        decimal Quantity,
        decimal UnitPrice,
        decimal TotalPrice);

    public sealed record StorefrontPackingOrderResponse(
        Guid Id,
        string Number,
        string CustomerName,
        string CustomerEmail,
        string? CustomerPhone,
        string? DeliveryAddress,
        Guid? AssignedWarehouseId,
        string AssignedWarehouseName,
        string Status,
        string CurrentLocation,
        DateTime CreatedOnUtc,
        DateTime? AcceptedOnUtc,
        DateTime? CompletedOnUtc,
        decimal TotalValue,
        IReadOnlyList<StorefrontPackingOrderItemResponse> Items);

    public sealed record StorefrontPackingOrderItemResponse(
        Guid Id,
        Guid ProductId,
        string ProductName,
        string Sku,
        decimal Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        IReadOnlyList<StorefrontPackingAllocationResponse> Allocations);

    public sealed record StorefrontPackingAllocationResponse(
        string LocationCode,
        decimal Quantity,
        decimal ConsumedQuantity);

    [HttpGet("products")]
    [EnableCors("PublicStorefront")]
    public async Task<ActionResult<IReadOnlyList<StorefrontProductResponse>>> Products(CancellationToken ct)
    {
        var products = await dbContext.Products.AsNoTracking().Where(product => product.IsActive)
            .OrderBy(product => product.Name).ToArrayAsync(ct);
        var quantities = await dbContext.Inventory.AsNoTracking().Where(item => item.Product.IsActive)
            .GroupBy(item => new { item.ProductId, item.Location.WarehouseId })
            .Select(group => new { group.Key.ProductId, Quantity = group.Sum(item => item.AvailableQuantity) })
            .ToArrayAsync(ct);
        // A single order must fit in one warehouse; do not advertise an unusable cross-warehouse sum.
        var available = quantities.GroupBy(item => item.ProductId).ToDictionary(group => group.Key, group => group.Max(item => item.Quantity));
        return Ok(products.Select(product => new StorefrontProductResponse(product.Id, product.Name,
            product.Sku, product.Barcode, product.Description, product.ImageUrl, product.SalePrice,
            available.GetValueOrDefault(product.Id))).ToArray());
    }

    [HttpPost("orders")]
    [EnableCors("PublicStorefront")]
    [EnableRateLimiting("checkout")]
    public Task<ActionResult<StorefrontOrderResponse>> CreateOrder([FromBody] StorefrontOrderRequest request, CancellationToken ct)
        => InTransactionAsync<StorefrontOrderResponse>(async () =>
    {
        // Checkouts touch the same stock and numbering rows; queue them instead of deadlocking.
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_getapplock @Resource = N'storefront-checkout', @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000", ct);
        if (string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.CustomerEmail))
            throw new CommandValidationException("Podaj dane klienta.");
        if (request.Items.Count == 0) throw new CommandValidationException("Koszyk jest pusty.");
        if (request.Items.Any(item => item.Quantity <= 0))
            throw new CommandValidationException("Ilość musi być większa od zera.");
        var requestedItems = request.Items.GroupBy(item => item.ProductId)
            .Select(group => new StorefrontOrderItemRequest(group.Key, group.Sum(item => item.Quantity)))
            .OrderBy(item => item.ProductId).ToArray();
        var requestedProductIds = requestedItems.Select(item => item.ProductId).ToArray();
        var unitSymbols = await dbContext.Products.AsNoTracking()
            .Where(product => requestedProductIds.Contains(product.Id))
            .Select(product => new { ProductId = product.Id, product.UnitOfMeasure.Symbol })
            .ToDictionaryAsync(product => product.ProductId, product => product.Symbol, ct);
        foreach (var item in requestedItems)
            QuantityRules.Ensure(item.Quantity, unitSymbols.GetValueOrDefault(item.ProductId));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {
            Name = request.CustomerName.Trim(), Email = request.CustomerEmail.Trim().ToLowerInvariant(),
            Phone = request.CustomerPhone?.Trim(), Address = request.DeliveryAddress?.Trim(),
            Notes = request.Notes?.Trim(), Items = requestedItems
        }))));
        var key = request.IdempotencyKey?.Trim();
        if (key is not null)
        {
            if (!Guid.TryParse(key, out var parsedKey) || parsedKey == Guid.Empty)
                throw new CommandValidationException("IdempotencyKey musi być losowym UUID.");
            key = parsedKey.ToString("N");
            var existing = await dbContext.StorefrontOrders.Include(order => order.Items)
                .Include(order => order.AssignedWarehouse).SingleOrDefaultAsync(order => order.IdempotencyKey == key, ct);
            if (existing is not null)
            {
                if (existing.RequestFingerprint != fingerprint)
                    throw new ResourceConflictException("Ten klucz żądania był już użyty z innymi danymi.");
                return Ok(OrderResponse(existing, existing.AssignedWarehouse?.Name ?? string.Empty));
            }
        }
        var productIds = requestedItems.Select(item => item.ProductId).ToArray();
        var products = await dbContext.Products.Where(product => product.IsActive && productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, ct);
        if (products.Count != requestedItems.Length)
            throw new CommandValidationException("Koszyk zawiera nieaktywny albo nieistniejący produkt.");
        var warehouse = await SelectWarehouseAsync(requestedItems, ct)
            ?? throw new CommandValidationException("Brak jednego magazynu, który może spakować całe zamówienie.");
        var now = DateTime.UtcNow;
        var order = new StorefrontOrder {
            Id = Guid.NewGuid(),
            Number = await numberGenerator.GenerateAsync(warehouse.WarehouseId, "SW", now, ct),
            TrackingToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
            IdempotencyKey = key, RequestFingerprint = fingerprint,
            CustomerName = request.CustomerName.Trim(), CustomerEmail = request.CustomerEmail.Trim().ToLowerInvariant(),
            CustomerPhone = request.CustomerPhone?.Trim(), DeliveryAddress = request.DeliveryAddress?.Trim(), Notes = request.Notes?.Trim(),
            CreatedOnUtc = now, ExpiresOnUtc = now.AddMinutes(30), AssignedWarehouseId = warehouse.WarehouseId,
            Status = StorefrontOrderStatus.New,
            Items = requestedItems.Select(item => new StorefrontOrderItem {
                Id = Guid.NewGuid(), ProductId = item.ProductId,
                Quantity = item.Quantity, UnitPrice = products[item.ProductId].SalePrice
            }).ToList()
        };
        dbContext.StorefrontOrders.Add(order);
        foreach (var item in order.Items) await ReserveItemAsync(order, item, warehouse, ct);
        await auditLogWriter.RecordAsync(new AuditLogRecord("Create", nameof(StorefrontOrder),
            order.Id.ToString(), $"Utworzono zamówienie {order.Number}."), ct);
        await dbContext.SaveChangesAsync(ct);
        return Created($"/api/Storefront/orders/{order.TrackingToken}/tracking", OrderResponse(order, warehouse.WarehouseName));
    }, ct);

    private static StorefrontOrderResponse OrderResponse(StorefrontOrder order, string warehouseName) => new(
        order.Id, order.Number, order.Items.Sum(item => item.Quantity * item.UnitPrice),
        order.AssignedWarehouseId!.Value, warehouseName, order.TrackingToken);

    [HttpGet("orders/packing")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<StorefrontPackingOrderResponse>>> PackingOrders(CancellationToken ct)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var orders = await PackingOrderQuery().Where(order => order.Status == StorefrontOrderStatus.New || order.Status == StorefrontOrderStatus.Accepted)
            .OrderBy(order => order.CreatedOnUtc).ToArrayAsync(ct);
        return Ok(orders.Select(ToPackingResponse).ToArray());
    }

    [HttpPost("orders/{id:guid}/accept")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public Task<ActionResult> AcceptPackingOrder(Guid id, CancellationToken ct) => InTransactionAsync(async () =>
    {
        var order = await PackingOrderAsync(id, ct);
        if (order.Status == StorefrontOrderStatus.Accepted) return NoContent();
        if (order.Status != StorefrontOrderStatus.New) throw new CommandValidationException("Zamówienie nie jest nowe.");
        if (order.ExpiresOnUtc <= DateTime.UtcNow)
        {
            await orderService.ReleaseAsync(order, "Upłynął termin rezerwacji.", ct);
            return BadRequest(new { message = "Rezerwacja zamówienia wygasła." });
        }
        order.Status = StorefrontOrderStatus.Accepted;
        order.AcceptedOnUtc = DateTime.UtcNow;
        order.ExpiresOnUtc = DateTime.UtcNow.AddHours(24);
        await auditLogWriter.RecordAsync(new AuditLogRecord("Accept", nameof(StorefrontOrder), id.ToString(), "Przyjęto do kompletacji."), ct);
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }, ct);

    [HttpPost("orders/{id:guid}/pack")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public Task<ActionResult> PackOrder(Guid id, CancellationToken ct) => InTransactionAsync(async () =>
    {
        var order = await PackingOrderAsync(id, ct);
        if (order.Status == StorefrontOrderStatus.Completed) return NoContent();
        if (order.Status != StorefrontOrderStatus.Accepted) throw new CommandValidationException("Najpierw przyjmij zamówienie do kompletacji.");
        if (order.ExpiresOnUtc <= DateTime.UtcNow)
        {
            await orderService.ReleaseAsync(order, "Upłynął termin kompletacji.", ct);
            return BadRequest(new { message = "Rezerwacja zamówienia wygasła." });
        }
        if (order.Items.Any(item => item.Allocations.Sum(allocation => allocation.Quantity) != item.Quantity))
            throw new ResourceConflictException("Alokacje nie pokrywają całego zamówienia.");
        foreach (var allocation in order.Items.SelectMany(item => item.Allocations))
        {
            var take = allocation.Quantity - allocation.ConsumedQuantity;
            if (take <= 0) continue;
            var inventory = await dbContext.Inventory.SingleAsync(item => item.Id == allocation.InventoryId, ct);
            if (inventory.ProductId != allocation.ProductId || inventory.LocationId != allocation.LocationId ||
                inventory.Quantity < take || inventory.ReservedQuantity < take)
                throw new ResourceConflictException("Stan nie pokrywa rezerwacji zamówienia.");
            var before = inventory.Quantity;
            var reserved = inventory.ReservedQuantity;
            inventory.Quantity -= take;
            inventory.ReservedQuantity -= take;
            allocation.ConsumedQuantity = allocation.Quantity;
            await publisher.Publish(new StockMovementRecordedEvent(allocation.WarehouseId, allocation.LocationId,
                allocation.ProductId, allocation.InventoryId, StockMovementType.TransferOut, before, -take, inventory.Quantity,
                reserved, -take, inventory.ReservedQuantity, "StorefrontOrder", id, order.Number, "Spakowanie zamówienia."), ct);
            await publisher.Publish(new InventoryChangedEvent(inventory.ProductId, inventory.LocationId,
                inventory.Quantity, inventory.ReservedQuantity, inventory.Id, -take), ct);
        }
        order.Status = StorefrontOrderStatus.Completed;
        order.CompletedOnUtc = DateTime.UtcNow;
        await auditLogWriter.RecordAsync(new AuditLogRecord("Pack", nameof(StorefrontOrder), id.ToString(), "Spakowano zamówienie."), ct);
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }, ct);

    [HttpPost("orders/{id:guid}/cancel")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public Task<ActionResult> CancelPackingOrder(Guid id, CancellationToken ct) => InTransactionAsync(async () =>
    {
        var order = await PackingOrderAsync(id, ct);
        if (order.Status == StorefrontOrderStatus.Completed) throw new CommandValidationException("Zrealizowanego zamówienia nie można anulować.");
        await orderService.ReleaseAsync(order, "Anulowanie przez obsługę magazynu.", ct);
        return NoContent();
    }, ct);

    [HttpPost("tracking/{identifier}/cancel")]
    [EnableCors("PublicStorefront")]
    [EnableRateLimiting("checkout")]
    public Task<ActionResult> CancelOrder(string identifier, CancellationToken ct) => InTransactionAsync(async () =>
    {
        var order = await orderService.OrdersWithAllocations().SingleOrDefaultAsync(item => item.TrackingToken == identifier, ct)
            ?? throw new ResourceNotFoundException("Nie znaleziono zamówienia.");
        if (order.Status == StorefrontOrderStatus.Cancelled) return NoContent();
        if (order.Status != StorefrontOrderStatus.New) throw new CommandValidationException("Zamówienie jest już obsługiwane; skontaktuj się z magazynem.");
        await orderService.ReleaseAsync(order, "Anulowanie przez klienta.", ct);
        return NoContent();
    }, ct);

    [HttpGet("orders/{identifier}/tracking")]
    [EnableCors("PublicStorefront")]
    [EnableRateLimiting("tracking")]
    public async Task<ActionResult<StorefrontTrackingResponse>> Tracking(string identifier, CancellationToken ct)
    {
        if (identifier.Length != 64 || !identifier.All(Uri.IsHexDigit)) return NotFound();
        var order = await dbContext.StorefrontOrders.AsNoTracking().Include(item => item.AssignedWarehouse)
            .Include(item => item.Items).ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.TrackingToken == identifier, ct);
        if (order is null) return NotFound(new { message = "Nie znaleziono zamówienia." });
        var items = order.Items.OrderBy(item => item.Product.Name).Select(item => new StorefrontTrackingItemResponse(
            item.Product.Name, item.Product.Sku, item.Quantity, item.UnitPrice, item.Quantity * item.UnitPrice)).ToArray();
        return Ok(new StorefrontTrackingResponse(order.Id, order.Number, DisplayStatus(order.Status),
            CurrentLocation(order), TrackingDescription(order.Status), order.CreatedOnUtc, items.Sum(item => item.TotalPrice), items));
    }

    private async Task<StorefrontOrder> PackingOrderAsync(Guid id, CancellationToken ct)
    {
        var order = await orderService.OrdersWithAllocations().SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new ResourceNotFoundException("Nie znaleziono zamówienia.");
        if (order.AssignedWarehouseId is not { } warehouseId || !warehouseContext.CanAccess(warehouseId))
            throw new ForbiddenOperationException("Brak dostępu do magazynu zamówienia.");
        return order;
    }

    private async Task<ActionResult<T>> InTransactionAsync<T>(Func<Task<ActionResult<T>>> action, CancellationToken ct)
    {
        ActionResult<T> result = default!;
        await unitOfWork.ExecuteInTransactionAsync(async _ => result = await action(), ct);
        return result;
    }
    private async Task<ActionResult> InTransactionAsync(Func<Task<ActionResult>> action, CancellationToken ct)
    {
        ActionResult result = default!;
        await unitOfWork.ExecuteInTransactionAsync(async _ => result = await action(), ct);
        return result;
    }

    private IQueryable<StorefrontOrder> PackingOrderQuery() => dbContext.StorefrontOrders
        .Where(order => warehouseContext.WarehouseId == null || order.AssignedWarehouseId == warehouseContext.WarehouseId)
        .AsNoTracking()
        .Include(order => order.AssignedWarehouse)
        .Include(order => order.Items)
        .ThenInclude(item => item.Product)
        .Include(order => order.Items)
        .ThenInclude(item => item.Allocations)
        .ThenInclude(allocation => allocation.Location);

    private static StorefrontPackingOrderResponse ToPackingResponse(StorefrontOrder order)
    {
        var items = new List<StorefrontPackingOrderItemResponse>();

        foreach (var item in order.Items.OrderBy(item => item.Product.Name))
        {
            var allocations = new List<StorefrontPackingAllocationResponse>();

            foreach (var allocation in item.Allocations.OrderBy(allocation => allocation.Location.LocationCode))
            {
                allocations.Add(new StorefrontPackingAllocationResponse(
                    allocation.Location.LocationCode,
                    allocation.Quantity,
                    allocation.ConsumedQuantity));
            }

            items.Add(new StorefrontPackingOrderItemResponse(
                item.Id,
                item.ProductId,
                item.Product.Name,
                item.Product.Sku,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice,
                allocations));
        }

        return new StorefrontPackingOrderResponse(
            order.Id,
            order.Number,
            order.CustomerName,
            order.CustomerEmail,
            order.CustomerPhone,
            order.DeliveryAddress,
            order.AssignedWarehouseId,
            order.AssignedWarehouse?.Name ?? "Nieprzypisany",
            DisplayStatus(order.Status),
            CurrentLocation(order),
            order.CreatedOnUtc,
            order.AcceptedOnUtc,
            order.CompletedOnUtc,
            items.Sum(item => item.TotalPrice),
            items);
    }

    private async Task<WarehouseSelection?> SelectWarehouseAsync(
        IReadOnlyCollection<StorefrontOrderItemRequest> requestedItems,
        CancellationToken cancellationToken)
    {
        var requestedByProduct = new Dictionary<Guid, decimal>();
        foreach (var item in requestedItems)
        {
            requestedByProduct[item.ProductId] = item.Quantity;
        }
        var requestedProductIds = requestedByProduct.Keys.ToArray();

        var inventories = await dbContext.Inventory
            .AsNoTracking()
            .Include(inventory => inventory.Location)
            .ThenInclude(location => location.Warehouse)
            .Where(inventory => requestedProductIds.Contains(inventory.ProductId))
            .ToArrayAsync(cancellationToken);

        WarehouseSelection? bestWarehouse = null;

        foreach (var warehouseGroup in inventories.GroupBy(inventory => inventory.Location.WarehouseId))
        {
            var availableByProduct = new Dictionary<Guid, decimal>();

            foreach (var inventory in warehouseGroup)
            {
                if (!availableByProduct.ContainsKey(inventory.ProductId))
                    availableByProduct[inventory.ProductId] = 0;

                availableByProduct[inventory.ProductId] += inventory.AvailableQuantity;
            }

            var canPackEverything = true;
            var surplus = 0m;

            foreach (var request in requestedByProduct)
            {
                var available = availableByProduct.GetValueOrDefault(request.Key);
                if (available < request.Value)
                {
                    canPackEverything = false;
                    break;
                }

                surplus += available - request.Value;
            }

            if (!canPackEverything)
                continue;

            var warehouse = warehouseGroup.First().Location.Warehouse;
            var candidate = new WarehouseSelection(
                warehouse.Id,
                warehouse.Name,
                availableByProduct,
                surplus);

            if (bestWarehouse is null
                || candidate.Surplus < bestWarehouse.Surplus
                || (candidate.Surplus == bestWarehouse.Surplus
                    && string.Compare(candidate.WarehouseName, bestWarehouse.WarehouseName, StringComparison.Ordinal) < 0))
            {
                bestWarehouse = candidate;
            }
        }

        return bestWarehouse;
    }

    private async Task ReserveItemAsync(
        StorefrontOrder order,
        StorefrontOrderItem item,
        WarehouseSelection selectedWarehouse,
        CancellationToken cancellationToken)
    {
        // Lock the rows while reading: concurrent orders for the same product must queue, not deadlock.
        var inventories = await dbContext.Inventory
            .FromSqlInterpolated($"SELECT * FROM [Inventory] WITH (UPDLOCK, ROWLOCK) WHERE [ProductId] = {item.ProductId}")
            .Include(inventory => inventory.Location)
            .Where(inventory =>
                inventory.ProductId == item.ProductId &&
                inventory.Location.WarehouseId == selectedWarehouse.WarehouseId)
            .OrderBy(inventory => inventory.Location.LocationCode)
            .ToArrayAsync(cancellationToken);

        var remaining = item.Quantity;
        foreach (var inventory in inventories)
        {
            if (remaining <= 0) break;

            var take = Math.Min(inventory.AvailableQuantity, remaining);
            if (take <= 0) continue;

            var reservedBefore = inventory.ReservedQuantity;
            inventory.ReservedQuantity += take;
            var allocation = new StorefrontOrderAllocation
            {
                Id = Guid.NewGuid(),
                StorefrontOrderItemId = item.Id,
                WarehouseId = selectedWarehouse.WarehouseId,
                LocationId = inventory.LocationId,
                InventoryId = inventory.Id,
                ProductId = item.ProductId,
                Quantity = take,
                ConsumedQuantity = 0
            };
            item.Allocations.Add(allocation);
            // The movement event below saves changes mid-way. Without an explicit state EF would treat an
            // allocation attached to an already saved order item as existing (key is set) and issue an UPDATE.
            dbContext.Entry(allocation).State = EntityState.Added;

            await publisher.Publish(new StockMovementRecordedEvent(selectedWarehouse.WarehouseId,
                inventory.LocationId, inventory.ProductId, inventory.Id, StockMovementType.Reservation,
                inventory.Quantity, 0, inventory.Quantity, reservedBefore, take, inventory.ReservedQuantity,
                "StorefrontOrder", order.Id, order.Number, "Rezerwacja zamówienia ze sklepu."), cancellationToken);
            await publisher.Publish(new InventoryChangedEvent(inventory.ProductId, inventory.LocationId,
                inventory.Quantity, inventory.ReservedQuantity, inventory.Id, 0), cancellationToken);
            remaining -= take;
        }

        if (remaining > 0)
            throw new CommandValidationException("Nie udało się zaalokować całego zamówienia.");
    }

    private static string DisplayStatus(StorefrontOrderStatus status) => status switch
    {
        StorefrontOrderStatus.New => "Nowe",
        StorefrontOrderStatus.Accepted => "Przyjęte do realizacji",
        StorefrontOrderStatus.Completed => "Zrealizowane",
        StorefrontOrderStatus.Cancelled => "Anulowane",
        _ => status.ToString()
    };

    private static string CurrentLocation(StorefrontOrder order) => order.Status switch
    {
        StorefrontOrderStatus.New => FormatLocation("Biuro obsługi zamówień", order.AssignedWarehouse?.Name),
        StorefrontOrderStatus.Accepted => FormatLocation("Magazyn kompletacji", order.AssignedWarehouse?.Name),
        StorefrontOrderStatus.Completed => "Wydane do klienta",
        StorefrontOrderStatus.Cancelled => "Zamówienie zamknięte",
        _ => "System Magazine"
    };

    private static string TrackingDescription(StorefrontOrderStatus status) => status switch
    {
        StorefrontOrderStatus.New => "Zamówienie zostało zapisane i czeka na potwierdzenie przez obsługę.",
        StorefrontOrderStatus.Accepted => "Zamówienie jest kompletowane w magazynie.",
        StorefrontOrderStatus.Completed => "Zamówienie zostało zakończone.",
        StorefrontOrderStatus.Cancelled => "Zamówienie zostało anulowane.",
        _ => "Status zamówienia został zarejestrowany w systemie."
    };

    private static string FormatLocation(string location, string? warehouseName) =>
        string.IsNullOrWhiteSpace(warehouseName)
            ? location
            : $"{location} - {warehouseName}";

    private sealed record WarehouseSelection(
        Guid WarehouseId,
        string WarehouseName,
        IReadOnlyDictionary<Guid, decimal> AvailableByProduct,
        decimal Surplus);
}
