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
        Guid? AssignedWarehouseId,
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
        IReadOnlyList<StorefrontTrackingItemResponse> Items,
        string? CarrierName = null,
        string? CarrierTrackingNumber = null);

    public sealed record StorefrontTrackingItemResponse(
        string ProductName,
        string Sku,
        decimal Quantity,
        decimal UnitPrice,
        decimal TotalPrice);

    public sealed record StorefrontPoolItemResponse(
        string ProductName,
        string Sku,
        decimal Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        decimal AvailableHere);

    public sealed record StorefrontPoolOrderResponse(
        Guid Id,
        string Number,
        string CustomerName,
        string? DeliveryAddress,
        string? Notes,
        DateTime CreatedOnUtc,
        DateTime ExpiresOnUtc,
        decimal TotalValue,
        bool CanFulfil,
        IReadOnlyList<StorefrontPoolItemResponse> Items);

    public sealed record ShipOrderRequest(string CarrierName, string? TrackingNumber);

    public sealed record StorefrontOrderHistoryResponse(
        Guid Id,
        string Number,
        string CustomerName,
        string CustomerEmail,
        string? CustomerPhone,
        string? DeliveryAddress,
        string? Notes,
        string AssignedWarehouseName,
        string? AssignedWarehouseAddress,
        string? AssignedWarehouseTaxNumber,
        string Status,
        DateTime CreatedOnUtc,
        DateTime? AcceptedOnUtc,
        string? AcceptedBy,
        DateTime? PackedOnUtc,
        string? PackedBy,
        DateTime? ShippedOnUtc,
        string? ShippedBy,
        string? CarrierName,
        string? CarrierTrackingNumber,
        decimal TotalValue,
        IReadOnlyList<StorefrontTrackingItemResponse> Items);

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
        decimal StockQuantity,
        IReadOnlyList<StorefrontPackingAllocationResponse> Allocations);

    public sealed record StorefrontPackingAllocationResponse(
        string LocationCode,
        decimal Quantity,
        decimal ConsumedQuantity);

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<StorefrontProductResponse>>> Products(CancellationToken ct)
    {
        var products = await dbContext.Products.AsNoTracking().Where(product => product.IsActive)
            .OrderBy(product => product.Name).ToArrayAsync(ct);
        var quantities = await dbContext.Inventory.AsNoTracking().Where(item => item.Product.IsActive)
            .GroupBy(item => new { item.ProductId, item.Location.WarehouseId })
            .Select(group => new { group.Key.ProductId, Quantity = group.Sum(item => item.AvailableQuantity) })
            .ToArrayAsync(ct);
        var available = quantities.GroupBy(item => item.ProductId).ToDictionary(group => group.Key, group => group.Max(item => item.Quantity));
        return Ok(products.Select(product => new StorefrontProductResponse(product.Id, product.Name,
            product.Sku, product.Barcode, product.Description, product.ImageUrl, product.SalePrice,
            available.GetValueOrDefault(product.Id))).ToArray());
    }

    [HttpPost("orders")]
    [EnableRateLimiting("checkout")]
    public Task<ActionResult<StorefrontOrderResponse>> CreateOrder([FromBody] StorefrontOrderRequest request, CancellationToken ct)
        => InTransactionAsync<StorefrontOrderResponse>(async () =>
    {
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
            Number = await numberGenerator.GenerateAsync(
                (await dbContext.Warehouses.AsNoTracking().Where(item => item.IsHub).Select(item => (Guid?)item.Id).FirstOrDefaultAsync(ct))
                    ?? warehouse.WarehouseId, "SW", now, ct),
            TrackingToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
            IdempotencyKey = key, RequestFingerprint = fingerprint,
            CustomerName = request.CustomerName.Trim(), CustomerEmail = request.CustomerEmail.Trim().ToLowerInvariant(),
            CustomerPhone = request.CustomerPhone?.Trim(), DeliveryAddress = request.DeliveryAddress?.Trim(), Notes = request.Notes?.Trim(),
            CreatedOnUtc = now, ExpiresOnUtc = now.AddHours(24), AssignedWarehouseId = null,
            Status = StorefrontOrderStatus.New,
            Items = requestedItems.Select(item => new StorefrontOrderItem {
                Id = Guid.NewGuid(), ProductId = item.ProductId,
                Quantity = item.Quantity, UnitPrice = products[item.ProductId].SalePrice
            }).ToList()
        };
        dbContext.StorefrontOrders.Add(order);
        await auditLogWriter.RecordAsync(new AuditLogRecord("Create", nameof(StorefrontOrder),
            order.Id.ToString(), $"Utworzono zamówienie {order.Number}."), ct);
        await dbContext.SaveChangesAsync(ct);
        return Created($"/api/Storefront/orders/{order.TrackingToken}/tracking", OrderResponse(order, string.Empty));
    }, ct);

    private static StorefrontOrderResponse OrderResponse(StorefrontOrder order, string warehouseName) => new(
        order.Id, order.Number, order.Items.Sum(item => item.Quantity * item.UnitPrice),
        order.AssignedWarehouseId, warehouseName, order.TrackingToken);

    [HttpGet("orders/packing")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<StorefrontPackingOrderResponse>>> PackingOrders(CancellationToken ct)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var orders = await PackingOrderQuery().Where(order => order.Status == StorefrontOrderStatus.New || order.Status == StorefrontOrderStatus.Accepted)
            .OrderBy(order => order.CreatedOnUtc).ToArrayAsync(ct);

        var warehouseIds = orders.Select(order => order.AssignedWarehouseId).OfType<Guid>().Distinct().ToArray();
        var productIds = orders.SelectMany(order => order.Items).Select(item => item.ProductId).Distinct().ToArray();
        var stock = await dbContext.Inventory.AsNoTracking()
            .Where(item => warehouseIds.Contains(item.Location.WarehouseId) && productIds.Contains(item.ProductId))
            .GroupBy(item => new { item.Location.WarehouseId, item.ProductId })
            .Select(group => new { group.Key.WarehouseId, group.Key.ProductId, Quantity = group.Sum(item => item.Quantity) })
            .ToDictionaryAsync(row => (row.WarehouseId, row.ProductId), row => row.Quantity, ct);
        return Ok(orders.Select(order => ToPackingResponse(order, stock)).ToArray());
    }

    [HttpGet("orders/pool")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<StorefrontPoolOrderResponse>>> Pool(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var orders = await dbContext.StorefrontOrders.AsNoTracking()
            .Include(order => order.Items).ThenInclude(item => item.Product)
            .Where(order => order.Status == StorefrontOrderStatus.New && order.AssignedWarehouseId == null && order.ExpiresOnUtc > now)
            .OrderBy(order => order.CreatedOnUtc).Take(200).ToArrayAsync(ct);

        var here = warehouseContext.WarehouseId;
        var productIds = orders.SelectMany(order => order.Items).Select(item => item.ProductId).Distinct().ToArray();
        var available = here is null
            ? new Dictionary<Guid, decimal>()
            : await dbContext.Inventory.AsNoTracking()
                .Where(item => item.Location.WarehouseId == here && productIds.Contains(item.ProductId))
                .GroupBy(item => item.ProductId)
                .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.AvailableQuantity) })
                .ToDictionaryAsync(row => row.ProductId, row => row.Quantity, ct);

        return Ok(orders.Select(order =>
        {
            var items = order.Items.OrderBy(item => item.Product.Name).Select(item => new StorefrontPoolItemResponse(
                item.Product.Name, item.Product.Sku, item.Quantity, item.UnitPrice, item.Quantity * item.UnitPrice,
                available.GetValueOrDefault(item.ProductId))).ToArray();
            return new StorefrontPoolOrderResponse(order.Id, order.Number, order.CustomerName, order.DeliveryAddress,
                order.Notes, order.CreatedOnUtc, order.ExpiresOnUtc, items.Sum(item => item.TotalPrice),
                here is not null && items.All(item => item.AvailableHere >= item.Quantity), items);
        }).ToArray());
    }

    [HttpPost("orders/{id:guid}/claim")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public Task<ActionResult> ClaimOrder(Guid id, CancellationToken ct) => InTransactionAsync(async () =>
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sp_getapplock @Resource = N'storefront-checkout', @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000", ct);
        var warehouseId = warehouseContext.WarehouseId
            ?? throw new CommandValidationException("Wybierz magazyn, który ma zrealizować zamówienie.");
        var warehouse = await dbContext.Warehouses.AsNoTracking().SingleAsync(item => item.Id == warehouseId, ct);
        var order = await orderService.OrdersWithAllocations().SingleOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new ResourceNotFoundException("Nie znaleziono zamówienia.");
        if (order.AssignedWarehouseId == warehouseId && order.Status != StorefrontOrderStatus.New) return NoContent();
        if (order.AssignedWarehouseId is not null || order.Status != StorefrontOrderStatus.New)
            throw new ResourceConflictException("Zamówienie zostało już przyjęte przez inny magazyn albo nie jest już dostępne.");
        if (order.ExpiresOnUtc <= DateTime.UtcNow)
        {
            await orderService.ReleaseAsync(order, "Upłynął termin przyjęcia zamówienia.", ct);
            return BadRequest(new { message = "Zamówienie wygasło." });
        }

        var selection = new WarehouseSelection(warehouse.Id, warehouse.Name, new Dictionary<Guid, decimal>(), 0);
        foreach (var item in order.Items) await ReserveItemAsync(order, item, selection, ct);
        order.AssignedWarehouseId = warehouse.Id;
        order.Status = StorefrontOrderStatus.Accepted;
        order.AcceptedOnUtc = DateTime.UtcNow;
        order.AcceptedByUserId = warehouseContext.UserId;
        order.ExpiresOnUtc = DateTime.UtcNow.AddHours(24);
        await auditLogWriter.RecordAsync(new AuditLogRecord("Claim", nameof(StorefrontOrder), id.ToString(),
            $"Zamówienie przyjął magazyn „{warehouse.Name}”."), ct);
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }, ct);

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
        order.AcceptedByUserId = warehouseContext.UserId;
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
        if (order.Status is StorefrontOrderStatus.Completed or StorefrontOrderStatus.Shipped) return NoContent();
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
        order.PackedByUserId = warehouseContext.UserId;
        await auditLogWriter.RecordAsync(new AuditLogRecord("Pack", nameof(StorefrontOrder), id.ToString(), "Spakowano zamówienie."), ct);
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }, ct);

    [HttpPost("orders/{id:guid}/ship")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public Task<ActionResult> ShipOrder(Guid id, [FromBody] ShipOrderRequest request, CancellationToken ct) => InTransactionAsync(async () =>
    {
        var carrier = request.CarrierName?.Trim();
        if (string.IsNullOrEmpty(carrier) || carrier.Length > 100)
            throw new CommandValidationException("Podaj przewoźnika (do 100 znaków).");
        var trackingNumber = string.IsNullOrWhiteSpace(request.TrackingNumber) ? null : request.TrackingNumber.Trim();
        if (trackingNumber is { Length: > 100 })
            throw new CommandValidationException("Numer przesyłki może mieć najwyżej 100 znaków.");

        var order = await PackingOrderAsync(id, ct);
        if (order.Status == StorefrontOrderStatus.Shipped) return NoContent();
        if (order.Status != StorefrontOrderStatus.Completed)
            throw new CommandValidationException("Wysłać można tylko spakowane zamówienie.");
        order.Status = StorefrontOrderStatus.Shipped;
        order.ShippedOnUtc = DateTime.UtcNow;
        order.ShippedByUserId = warehouseContext.UserId;
        order.CarrierName = carrier;
        order.CarrierTrackingNumber = trackingNumber;
        await auditLogWriter.RecordAsync(new AuditLogRecord("Ship", nameof(StorefrontOrder), id.ToString(),
            $"Przekazano przewoźnikowi: {carrier}{(trackingNumber is null ? string.Empty : $", przesyłka {trackingNumber}")}."), ct);
        await dbContext.SaveChangesAsync(ct);
        return NoContent();
    }, ct);

    [HttpGet("orders/history")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<StorefrontOrderHistoryResponse>>> OrderHistory(
        [FromQuery] int? status, [FromQuery] int days = 90, CancellationToken ct = default)
    {
        if (!warehouseContext.IsAdministrator && warehouseContext.WarehouseId is null)
            throw new ForbiddenOperationException("Użytkownik nie ma przypisanego magazynu.");
        var fromUtc = DateTime.UtcNow.AddDays(-Math.Clamp(days, 1, 730));
        var query = PackingOrderQuery().Where(order => order.CreatedOnUtc >= fromUtc);
        if (status is { } value && Enum.IsDefined(typeof(StorefrontOrderStatus), value))
            query = query.Where(order => order.Status == (StorefrontOrderStatus)value);
        var orders = await query.OrderByDescending(order => order.CreatedOnUtc).Take(500).ToArrayAsync(ct);

        var userIds = orders.SelectMany(order => new[] { order.AcceptedByUserId, order.PackedByUserId, order.ShippedByUserId })
            .OfType<Guid>().Distinct().ToArray();
        var users = await dbContext.Users.AsNoTracking().Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => $"{user.FirstName} {user.LastName}".Trim(), ct);
        string? Name(Guid? id) => id is { } key ? users.GetValueOrDefault(key, "Usunięty użytkownik") : null;

        return Ok(orders.Select(order =>
        {
            var items = order.Items.OrderBy(item => item.Product.Name).Select(item => new StorefrontTrackingItemResponse(
                item.Product.Name, item.Product.Sku, item.Quantity, item.UnitPrice, item.Quantity * item.UnitPrice)).ToArray();
            return new StorefrontOrderHistoryResponse(
                order.Id, order.Number, order.CustomerName, order.CustomerEmail, order.CustomerPhone, order.DeliveryAddress,
                order.Notes, order.AssignedWarehouse?.Name ?? "Nieprzypisany", order.AssignedWarehouse?.Address,
                order.AssignedWarehouse?.TaxNumber, DisplayStatus(order.Status), order.CreatedOnUtc,
                order.AcceptedOnUtc, Name(order.AcceptedByUserId), order.CompletedOnUtc, Name(order.PackedByUserId),
                order.ShippedOnUtc, Name(order.ShippedByUserId), order.CarrierName, order.CarrierTrackingNumber,
                items.Sum(item => item.TotalPrice), items);
        }).ToArray());
    }

    [HttpPost("orders/{id:guid}/cancel")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public Task<ActionResult> CancelPackingOrder(Guid id, CancellationToken ct) => InTransactionAsync(async () =>
    {
        var order = await PackingOrderAsync(id, ct);
        if (order.Status is StorefrontOrderStatus.Completed or StorefrontOrderStatus.Shipped)
            throw new CommandValidationException("Spakowanego zamówienia nie można anulować.");
        await orderService.ReleaseAsync(order, "Anulowanie przez obsługę magazynu.", ct);
        return NoContent();
    }, ct);

    [HttpPost("tracking/{identifier}/cancel")]
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
            CurrentLocation(order), TrackingDescription(order.Status), order.CreatedOnUtc, items.Sum(item => item.TotalPrice), items,
            order.CarrierName, order.CarrierTrackingNumber));
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

    private static StorefrontPackingOrderResponse ToPackingResponse(
        StorefrontOrder order, IReadOnlyDictionary<(Guid WarehouseId, Guid ProductId), decimal> stock)
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
                order.AssignedWarehouseId is { } warehouseId ? stock.GetValueOrDefault((warehouseId, item.ProductId)) : 0,
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
            throw new CommandValidationException("Magazyn nie ma wystarczającego stanu dla całego zamówienia.");
    }

    private static string DisplayStatus(StorefrontOrderStatus status) => status switch
    {
        StorefrontOrderStatus.New => "Nowe",
        StorefrontOrderStatus.Accepted => "Przyjęte do realizacji",
        StorefrontOrderStatus.Completed => "Spakowane",
        StorefrontOrderStatus.Shipped => "Wysłane",
        StorefrontOrderStatus.Cancelled => "Anulowane",
        _ => status.ToString()
    };

    private static string CurrentLocation(StorefrontOrder order) => order.Status switch
    {
        StorefrontOrderStatus.New => FormatLocation("Biuro obsługi zamówień", order.AssignedWarehouse?.Name),
        StorefrontOrderStatus.Accepted => FormatLocation("Magazyn kompletacji", order.AssignedWarehouse?.Name),
        StorefrontOrderStatus.Completed => FormatLocation("Strefa wysyłek", order.AssignedWarehouse?.Name),
        StorefrontOrderStatus.Shipped => string.IsNullOrWhiteSpace(order.CarrierName) ? "Przekazane przewoźnikowi" : $"Przekazane przewoźnikowi: {order.CarrierName}",
        StorefrontOrderStatus.Cancelled => "Zamówienie zamknięte",
        _ => "System Magazine"
    };

    private static string TrackingDescription(StorefrontOrderStatus status) => status switch
    {
        StorefrontOrderStatus.New => "Zamówienie zostało zapisane i czeka na przyjęcie przez jeden z magazynów.",
        StorefrontOrderStatus.Accepted => "Zamówienie jest kompletowane w magazynie.",
        StorefrontOrderStatus.Completed => "Zamówienie zostało spakowane i czeka na przekazanie kurierowi.",
        StorefrontOrderStatus.Shipped => "Zamówienie zostało przekazane przewoźnikowi.",
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
