using MagazineAPI.Authorization;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using MagazineAPInfrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StorefrontController(
    AppDbContext dbContext,
    IWarehouseContext warehouseContext,
    IStockMovementWriter stockMovementWriter) : ControllerBase
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
        IReadOnlyList<StorefrontOrderItemRequest> Items);

    public sealed record StorefrontOrderItemRequest(Guid ProductId, decimal Quantity);

    public sealed record StorefrontOrderResponse(
        Guid Id,
        string Number,
        decimal TotalValue,
        Guid AssignedWarehouseId,
        string AssignedWarehouseName);

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
    public async Task<ActionResult<IReadOnlyList<StorefrontProductResponse>>> Products(
        CancellationToken cancellationToken)
    {
        var availableByProduct = await dbContext.Inventory
            .AsNoTracking()
            .GroupBy(inventory => inventory.ProductId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Sum(inventory => inventory.AvailableQuantity),
                cancellationToken);

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderBy(product => product.Name)
            .ToArrayAsync(cancellationToken);

        return Ok(products.Select(product => new StorefrontProductResponse(
            product.ProductId,
            product.Name,
            product.Sku,
            product.Barcode,
            product.Description,
            product.ImageUrl,
            product.SalePrice,
            availableByProduct.GetValueOrDefault(product.ProductId))).ToArray());
    }

    [HttpPost("orders")]
    public async Task<ActionResult<StorefrontOrderResponse>> CreateOrder(
        [FromBody] StorefrontOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return BadRequest(new { message = "Podaj imię i nazwisko." });
        if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
            return BadRequest(new { message = "Podaj poprawny adres e-mail." });
        if (request.Items.Count == 0)
            return BadRequest(new { message = "Koszyk jest pusty." });

        var requestedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new StorefrontOrderItemRequest(group.Key, group.Sum(item => item.Quantity)))
            .ToArray();
        var requestedProductIds = requestedItems.Select(item => item.ProductId).ToHashSet();
        var products = await dbContext.Products
            .Where(product => product.IsActive && requestedProductIds.Contains(product.ProductId))
            .ToDictionaryAsync(product => product.ProductId, cancellationToken);

        var selectedWarehouse = await SelectWarehouseAsync(requestedItems, cancellationToken);
        if (selectedWarehouse is null)
            return BadRequest(new
            {
                message = "Brak jednego magazynu, który może samodzielnie spakować całe zamówienie."
            });

        var items = new List<StorefrontOrderItem>();
        foreach (var requestItem in requestedItems)
        {
            if (requestItem.Quantity <= 0)
                return BadRequest(new { message = "Ilość musi być większa od zera." });
            if (!products.TryGetValue(requestItem.ProductId, out var product))
                return BadRequest(new { message = "Koszyk zawiera nieaktywny albo nieistniejący produkt." });
            if (selectedWarehouse.AvailableByProduct.GetValueOrDefault(requestItem.ProductId) < requestItem.Quantity)
                return BadRequest(new { message = $"Brak wystarczającego stanu dla produktu {product.Name}." });

            items.Add(new StorefrontOrderItem
            {
                StorefrontOrderItemId = Guid.NewGuid(),
                ProductId = product.ProductId,
                Quantity = requestItem.Quantity,
                UnitPrice = product.SalePrice
            });
        }

        var now = DateTime.UtcNow;
        var order = new StorefrontOrder
        {
            StorefrontOrderId = Guid.NewGuid(),
            Number = $"SW/{now:yyyyMMdd}/{Random.Shared.Next(1000, 10000)}",
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim().ToLowerInvariant(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            DeliveryAddress = request.DeliveryAddress?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedOnUtc = now,
            AssignedWarehouseId = selectedWarehouse.WarehouseId,
            Status = StorefrontOrderStatus.New,
            Items = items
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.StorefrontOrders.Add(order);

        foreach (var item in items)
        {
            await ReserveItemAsync(order, item, selectedWarehouse, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Created(
            $"/api/Storefront/orders/{order.StorefrontOrderId}",
            new StorefrontOrderResponse(
                order.StorefrontOrderId,
                order.Number,
                items.Sum(item => item.Quantity * item.UnitPrice),
                selectedWarehouse.WarehouseId,
                selectedWarehouse.WarehouseName));
    }

    [HttpGet("orders/packing")]
    [HasPermission(PermissionCodes.ShipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<StorefrontPackingOrderResponse>>> PackingOrders(
        CancellationToken cancellationToken)
    {
        var orders = await PackingOrderQuery()
            .Where(order => order.Status == StorefrontOrderStatus.New ||
                            order.Status == StorefrontOrderStatus.Accepted)
            .OrderBy(order => order.CreatedOnUtc)
            .ToArrayAsync(cancellationToken);

        return Ok(orders
            .Where(order => order.AssignedWarehouseId is null ||
                            warehouseContext.CanAccess(order.AssignedWarehouseId.Value))
            .Select(ToPackingResponse)
            .ToArray());
    }

    [HttpPost("orders/{id:guid}/accept")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public async Task<IActionResult> AcceptPackingOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await dbContext.StorefrontOrders
            .FirstOrDefaultAsync(candidate => candidate.StorefrontOrderId == id, cancellationToken);

        if (order is null)
            return NotFound(new { message = "Nie znaleziono zamówienia." });
        if (order.Status != StorefrontOrderStatus.New)
            return BadRequest(new { message = "Tylko nowe zamówienie można przyjąć do kompletacji." });
        if (order.AssignedWarehouseId is null)
            return BadRequest(new { message = "Zamówienie nie ma przypisanego magazynu pakowania." });
        if (!warehouseContext.CanAccess(order.AssignedWarehouseId.Value))
            throw new ForbiddenOperationException("Brak dostępu do magazynu przypisanego do zamówienia.");

        order.Status = StorefrontOrderStatus.Accepted;
        order.AcceptedOnUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("orders/{id:guid}/pack")]
    [HasPermission(PermissionCodes.ShipmentsApprove)]
    public async Task<IActionResult> PackOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await dbContext.StorefrontOrders
            .Include(candidate => candidate.Items)
            .ThenInclude(item => item.Allocations)
            .FirstOrDefaultAsync(candidate => candidate.StorefrontOrderId == id, cancellationToken);

        if (order is null)
            return NotFound(new { message = "Nie znaleziono zamówienia." });
        if (order.Status != StorefrontOrderStatus.Accepted)
            return BadRequest(new { message = "Najpierw przyjmij zamówienie do kompletacji." });
        if (order.AssignedWarehouseId is null)
            return BadRequest(new { message = "Zamówienie nie ma przypisanego magazynu pakowania." });
        if (!warehouseContext.CanAccess(order.AssignedWarehouseId.Value))
            throw new ForbiddenOperationException("Brak dostępu do magazynu przypisanego do zamówienia.");

        var allocations = order.Items.SelectMany(item => item.Allocations).ToArray();
        if (allocations.Length == 0)
            return BadRequest(new { message = "Zamówienie nie ma alokacji magazynowych." });

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var allocation in allocations)
        {
            var take = allocation.Quantity - allocation.ConsumedQuantity;
            if (take <= 0) continue;

            var inventory = await dbContext.Inventory
                .FirstOrDefaultAsync(candidate => candidate.InventoryId == allocation.InventoryId, cancellationToken);

            if (inventory is null)
                return BadRequest(new { message = "Nie znaleziono stanu powiązanego z alokacją." });
            if (inventory.Quantity < take || inventory.ReservedQuantity < take)
                return BadRequest(new { message = "Stan magazynowy jest niższy niż rezerwacja zamówienia." });

            var quantityBefore = inventory.Quantity;
            var reservedQuantityBefore = inventory.ReservedQuantity;
            inventory.Quantity -= take;
            inventory.ReservedQuantity -= take;
            allocation.ConsumedQuantity = allocation.Quantity;

            await stockMovementWriter.RecordAsync(
                new StockMovementRecord(
                    allocation.WarehouseId,
                    allocation.LocationId,
                    allocation.ProductId,
                    allocation.InventoryId,
                    StockMovementType.TransferOut,
                    quantityBefore,
                    -take,
                    inventory.Quantity,
                    reservedQuantityBefore,
                    -take,
                    inventory.ReservedQuantity,
                    "StorefrontOrder",
                    order.StorefrontOrderId,
                    order.Number,
                    "Spakowanie zamówienia ze sklepu."),
                cancellationToken);
        }

        order.Status = StorefrontOrderStatus.Completed;
        order.CompletedOnUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("orders/{identifier}/tracking")]
    public async Task<ActionResult<StorefrontTrackingResponse>> Tracking(
        string identifier,
        CancellationToken cancellationToken)
    {
        var normalizedIdentifier = Uri.UnescapeDataString(identifier).Trim();
        var orderQuery = dbContext.StorefrontOrders
            .AsNoTracking()
            .Include(order => order.AssignedWarehouse)
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .AsQueryable();

        var order = Guid.TryParse(normalizedIdentifier, out var orderId)
            ? await orderQuery.FirstOrDefaultAsync(
                item => item.StorefrontOrderId == orderId,
                cancellationToken)
            : await orderQuery.FirstOrDefaultAsync(
                item => item.Number == normalizedIdentifier,
                cancellationToken);

        if (order is null)
            return NotFound(new { message = "Nie znaleziono zamówienia." });

        var items = order.Items
            .OrderBy(item => item.Product.Name)
            .Select(item => new StorefrontTrackingItemResponse(
                item.Product.Name,
                item.Product.Sku,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice))
            .ToArray();

        return Ok(new StorefrontTrackingResponse(
            order.StorefrontOrderId,
            order.Number,
            DisplayStatus(order.Status),
            CurrentLocation(order),
            TrackingDescription(order.Status),
            order.CreatedOnUtc,
            items.Sum(item => item.TotalPrice),
            items));
    }

    private IQueryable<StorefrontOrder> PackingOrderQuery() => dbContext.StorefrontOrders
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
                item.StorefrontOrderItemId,
                item.ProductId,
                item.Product.Name,
                item.Product.Sku,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice,
                allocations));
        }

        return new StorefrontPackingOrderResponse(
            order.StorefrontOrderId,
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
                warehouse.WarehouseId,
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

            inventory.ReservedQuantity += take;
            item.Allocations.Add(new StorefrontOrderAllocation
            {
                StorefrontOrderAllocationId = Guid.NewGuid(),
                StorefrontOrderItemId = item.StorefrontOrderItemId,
                WarehouseId = selectedWarehouse.WarehouseId,
                LocationId = inventory.LocationId,
                InventoryId = inventory.InventoryId,
                ProductId = item.ProductId,
                Quantity = take,
                ConsumedQuantity = 0
            });

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
