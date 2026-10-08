using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MagazineWarehouseBot.Models;

namespace MagazineWarehouseBot.Api;

public sealed class MagazineApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AuthResponse> LoginAsync(string login, string password, CancellationToken cancellationToken)
    {
        return await SendAsync<AuthResponse>(
            HttpMethod.Post,
            "Auth/login",
            null,
            new { login, password },
            cancellationToken) ?? throw new ApiException("Logowanie nie zwrocilo tokenu.");
    }

    public Task<UserView?> MeAsync(string token, CancellationToken cancellationToken) =>
        SendAsync<UserView>(HttpMethod.Get, "Auth/me", token, null, cancellationToken);

    public async Task<IReadOnlySet<string>> PermissionsAsync(string token, CancellationToken cancellationToken)
    {
        var response = await SendAsync<PermissionsResponse>(
            HttpMethod.Get,
            "Auth/me/permissions",
            token,
            null,
            cancellationToken);
        return response?.PermissionCodes.ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<WarehouseView>> WarehousesAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<WarehouseView>>(HttpMethod.Get, "Warehouses", token, null, cancellationToken) ?? [];

    public async Task<WarehouseView?> CreateWarehouseAsync(
        string token,
        WarehouseSaveRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<WarehouseView>(HttpMethod.Post, "Warehouses", token, request, cancellationToken);

    public async Task<IReadOnlyList<LocationView>> LocationsAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<LocationView>>(HttpMethod.Get, "Locations", token, null, cancellationToken) ?? [];

    public async Task<LocationView?> CreateLocationAsync(
        string token,
        LocationSaveRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<LocationView>(HttpMethod.Post, "Locations", token, request, cancellationToken);

    public async Task<IReadOnlyList<CategoryView>> CategoriesAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<CategoryView>>(HttpMethod.Get, "Category", token, null, cancellationToken) ?? [];

    public async Task<CategoryView?> CreateCategoryAsync(
        string token,
        CategorySaveRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<CategoryView>(HttpMethod.Post, "Category", token, request, cancellationToken);

    public async Task<IReadOnlyList<UnitOfMeasureView>> UnitsAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<UnitOfMeasureView>>(HttpMethod.Get, "UnitsOfMeasure", token, null, cancellationToken) ?? [];

    public async Task<UnitOfMeasureView?> CreateUnitAsync(
        string token,
        UnitOfMeasureSaveRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<UnitOfMeasureView>(HttpMethod.Post, "UnitsOfMeasure", token, request, cancellationToken);

    public async Task<IReadOnlyList<ContractorView>> ContractorsAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<ContractorView>>(HttpMethod.Get, "Contractors", token, null, cancellationToken) ?? [];

    public async Task<ContractorView?> CreateContractorAsync(
        string token,
        ContractorSaveRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<ContractorView>(HttpMethod.Post, "Contractors", token, request, cancellationToken);

    public async Task<IReadOnlyList<RoleView>> RolesAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<RoleView>>(HttpMethod.Get, "Roles", token, null, cancellationToken) ?? [];

    public async Task<IReadOnlyList<UserView>> UsersAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<UserView>>(HttpMethod.Get, "Users", token, null, cancellationToken) ?? [];

    public async Task<UserView?> CreateUserAsync(
        string token,
        UserSaveRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<UserView>(HttpMethod.Post, "Users", token, request, cancellationToken);

    public async Task<ProductPageData?> ProductPageDataAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<ProductPageData>(HttpMethod.Get, "Products/page-data", token, null, cancellationToken);

    public async Task<PaginationReadModel<ProductReadModel>> ProductsAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<PaginationReadModel<ProductReadModel>>(
            HttpMethod.Get,
            "Products?page=1&pageSize=100&sortBy=name&order=asc",
            token,
            null,
            cancellationToken)
        ?? new PaginationReadModel<ProductReadModel>(0, [], 1, 100);

    public async Task<ProductReadModel?> CreateProductAsync(
        string token,
        ProductSaveRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<ProductReadModel>(HttpMethod.Post, "Products", token, request, cancellationToken);

    public Task DeleteProductAsync(string token, Guid productId, CancellationToken cancellationToken) =>
        SendNoContentAsync(HttpMethod.Delete, $"Products/{productId}", token, null, null, cancellationToken);

    public async Task<IReadOnlyList<InventoryView>> InventoryAsync(
        string token,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<InventoryView>>(
            HttpMethod.Get,
            "Inventory",
            token,
            null,
            cancellationToken,
            warehouseContextId) ?? [];

    public async Task<InventoryPageDataView?> InventoryPageDataAsync(
        string token,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<InventoryPageDataView>(
            HttpMethod.Get,
            "Inventory/page-data",
            token,
            null,
            cancellationToken,
            warehouseContextId);

    public async Task<InventoryView?> CreateInventoryAsync(
        string token,
        InventorySaveRequest request,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<InventoryView>(
            HttpMethod.Post,
            "Inventory",
            token,
            request,
            cancellationToken,
            warehouseContextId);

    public async Task<InventoryView?> UpdateInventoryAsync(
        string token,
        Guid inventoryId,
        InventorySaveRequest request,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<InventoryView>(
            HttpMethod.Put,
            $"Inventory/{inventoryId}",
            token,
            request,
            cancellationToken,
            warehouseContextId);

    public Task DeleteInventoryAsync(string token, Guid inventoryId, Guid? warehouseContextId, CancellationToken cancellationToken) =>
        SendNoContentAsync(HttpMethod.Delete, $"Inventory/{inventoryId}", token, null, warehouseContextId, cancellationToken);

    public async Task<CreatedIdResponse?> CompleteWarehouseOperationAsync(
        string token,
        WarehouseOperationRequest request,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<CreatedIdResponse>(
            HttpMethod.Post,
            "WarehouseOperations/complete",
            token,
            request,
            cancellationToken,
            warehouseContextId);

    public async Task<ShipmentPageDataView?> ShipmentPageDataAsync(
        string token,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<ShipmentPageDataView>(
            HttpMethod.Get,
            "Shipments/page-data",
            token,
            null,
            cancellationToken,
            warehouseContextId);

    public async Task<IReadOnlyList<ShipmentView>> ShipmentsAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<ShipmentView>>(HttpMethod.Get, "Shipments", token, null, cancellationToken) ?? [];

    public async Task<ShipmentView?> CreateShipmentAsync(
        string token,
        CreateShipmentRequest request,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<ShipmentView>(
            HttpMethod.Post,
            "Shipments",
            token,
            request,
            cancellationToken,
            warehouseContextId);

    public async Task<ShipmentView?> CreateShipmentRequestAsync(
        string token,
        CreateShipmentDemandRequest request,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<ShipmentView>(
            HttpMethod.Post,
            "Shipments/request",
            token,
            request,
            cancellationToken,
            warehouseContextId);

    public Task ApproveShipmentAsync(string token, Guid shipmentId, CancellationToken cancellationToken) =>
        SendNoContentAsync(HttpMethod.Post, $"Shipments/{shipmentId}/approve", token, null, null, cancellationToken);

    public Task MarkShipmentInTransitAsync(string token, Guid shipmentId, CancellationToken cancellationToken) =>
        SendNoContentAsync(HttpMethod.Post, $"Shipments/{shipmentId}/in-transit", token, null, null, cancellationToken);

    public Task ReceiveShipmentAsync(
        string token,
        Guid shipmentId,
        ReceiveShipmentRequest request,
        CancellationToken cancellationToken) =>
        SendNoContentAsync(HttpMethod.Post, $"Shipments/{shipmentId}/receive", token, request, null, cancellationToken);

    public async Task<PurchaseOrderPageDataView?> PurchaseOrderPageDataAsync(
        string token,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<PurchaseOrderPageDataView>(
            HttpMethod.Get,
            "PurchaseOrders/page-data",
            token,
            null,
            cancellationToken,
            warehouseContextId);

    public async Task<IReadOnlyList<PurchaseOrderView>> PurchaseOrdersAsync(string token, CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<PurchaseOrderView>>(HttpMethod.Get, "PurchaseOrders", token, null, cancellationToken) ?? [];

    public async Task<PurchaseOrderView?> CreatePurchaseOrderAsync(
        string token,
        CreatePurchaseOrderRequest request,
        Guid? warehouseContextId,
        CancellationToken cancellationToken) =>
        await SendAsync<PurchaseOrderView>(
            HttpMethod.Post,
            "PurchaseOrders",
            token,
            request,
            cancellationToken,
            warehouseContextId);

    public Task ApprovePurchaseOrderAsync(
        string token,
        Guid orderId,
        ApprovePurchaseOrderRequest request,
        CancellationToken cancellationToken) =>
        SendNoContentAsync(HttpMethod.Post, $"PurchaseOrders/{orderId}/approve", token, request, null, cancellationToken);

    public Task ReceivePurchaseOrderAsync(
        string token,
        Guid orderId,
        ReceivePurchaseOrderRequest request,
        CancellationToken cancellationToken) =>
        SendNoContentAsync(HttpMethod.Post, $"PurchaseOrders/{orderId}/receive", token, request, null, cancellationToken);

    private async Task SendNoContentAsync(
        HttpMethod method,
        string path,
        string? token,
        object? body,
        Guid? warehouseContextId,
        CancellationToken cancellationToken)
    {
        _ = await SendAsync<object>(method, path, token, body, cancellationToken, warehouseContextId);
    }

    private async Task<T?> SendAsync<T>(
        HttpMethod method,
        string path,
        string? token,
        object? body,
        CancellationToken cancellationToken,
        Guid? warehouseContextId = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (warehouseContextId is not null)
        {
            request.Headers.Add("X-Warehouse-Id", warehouseContextId.Value.ToString());
        }

        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException($"{(int)response.StatusCode} {response.ReasonPhrase}: {TrimContent(content)}");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    private static string TrimContent(string content)
    {
        content = content.ReplaceLineEndings(" ").Trim();
        return content.Length <= 500 ? content : $"{content[..500]}...";
    }
}

public sealed class ApiException(string message) : Exception(message);
