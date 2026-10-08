namespace MagazineWarehouseBot.Models;

public sealed record AuthResponse(string Token);
public sealed record PermissionsResponse(IReadOnlyList<string> PermissionCodes);
public sealed record UserView(Guid Id, string Login, string FirstName, string LastName,
    string Email, Guid RoleId, string RoleName, Guid? WarehouseId, string? WarehouseName);
public sealed record RoleView(Guid Id, string Name, bool HasAllPermissions, IReadOnlyList<string> PermissionCodes);
public sealed record UserSaveRequest(string Login, string FirstName, string LastName, string Email,
    string Password, Guid RoleId, Guid? WarehouseId);

public sealed record PaginationReadModel<T>(int Total, List<T> List, int Page, int PageSize);
public sealed record CategoryView(Guid Id, string Name, string? Description);
public sealed record UnitOfMeasureView(Guid Id, string Name, string Symbol);
public sealed record CategorySaveRequest(Guid? Id, string Name, string? Description);
public sealed record UnitOfMeasureSaveRequest(Guid? Id, string Name, string Symbol);
public sealed record ContractorView(Guid Id, string Name, string TaxNumber, int Type, string? Email, string? Phone, string? Address);
public sealed record ContractorSaveRequest(Guid? Id, string Name, string TaxNumber, int Type, string? Email, string? Phone, string? Address);
public sealed record ProductPageData(IReadOnlyList<CategoryView> Categories, IReadOnlyList<UnitOfMeasureView> Units);
public sealed record ProductReadModel(Guid ProductId, Guid CategoryId, Guid UnitOfMeasureId,
    string Name, string Sku, string? Barcode, string? Description, string? ImageUrl, string? UnitOfMeasure,
    string Category, decimal PurchasePrice, decimal SalePrice, bool IsActive);

public sealed record ProductSaveRequest(string Name, string Sku, string? Barcode, string? Description,
    string? ImageUrl, Guid UnitOfMeasureId, Guid CategoryId, decimal PurchasePrice, decimal SalePrice, bool IsActive);

public sealed record WarehouseView(Guid Id, string Name, string? Address, string? Description);
public sealed record LocationView(Guid Id, Guid WarehouseId, string WarehouseName, string LocationCode, string? Description);
public sealed record WarehouseSaveRequest(Guid? Id, string Name, string? Address, string? Description);
public sealed record LocationSaveRequest(Guid? Id, Guid WarehouseId, string LocationCode, string? Description);

public sealed record ProductOption(Guid Id, string Name, string Sku);
public sealed record LocationOption(Guid Id, string Code, Guid WarehouseId, string WarehouseName);
public sealed record InventoryPageDataView(IReadOnlyList<ProductOption> Products, IReadOnlyList<LocationOption> Locations);
public sealed record InventoryView(Guid Id, Guid ProductId, string ProductName, Guid LocationId,
    string LocationCode, Guid WarehouseId, string WarehouseName,
    decimal Quantity, decimal ReservedQuantity, decimal AvailableQuantity);
public sealed record InventorySaveRequest(Guid? Id, Guid ProductId, Guid LocationId, decimal Quantity, decimal ReservedQuantity);
public sealed record WarehouseOperationItemRequest(Guid ProductId, Guid? SourceLocationId,
    Guid? DestinationLocationId, decimal Quantity, decimal? TargetQuantity);
public sealed record WarehouseOperationRequest(int Type, Guid? WarehouseId, string? Notes,
    IReadOnlyList<WarehouseOperationItemRequest> Items);
public sealed record CreatedIdResponse(Guid Id);

public sealed record ShipmentPageDataView(WarehouseView SourceWarehouse,
    IReadOnlyList<WarehouseView> DestinationWarehouses,
    IReadOnlyList<ShipmentProductView> AvailableProducts);
public sealed record ShipmentView(Guid Id, string Number, Guid SourceWarehouseId, string SourceWarehouseName,
    Guid DestinationWarehouseId, string DestinationWarehouseName, string Status, DateTime CreatedOnUtc,
    DateTime? ApprovedOnUtc, DateTime? ReceivedOnUtc, IReadOnlyList<ShipmentItemView> Items);
public sealed record ShipmentItemView(Guid Id, Guid ProductId, string ProductName, string Sku, string Barcode, decimal Quantity);
public sealed record ShipmentProductView(Guid ProductId, string Name, string Sku, string Barcode, decimal AvailableQuantity);
public sealed record CreateShipmentItem(string? Barcode, Guid? ProductId, decimal Quantity);
public sealed record CreateShipmentRequest(Guid DestinationWarehouseId, IReadOnlyList<CreateShipmentItem> Items);
public sealed record CreateShipmentDemandRequest(IReadOnlyList<CreateShipmentItem> Items);
public sealed record ReceiveShipmentRequest(IReadOnlyCollection<Guid> CheckedItemIds, string? Notes);

public sealed record PurchaseOrderPageDataView(WarehouseView Warehouse,
    IReadOnlyList<PurchaseOrderContractorView> Suppliers,
    IReadOnlyList<PurchaseOrderProductView> Products);
public sealed record PurchaseOrderContractorView(Guid Id, string Name, string TaxNumber);
public sealed record PurchaseOrderProductView(Guid ProductId, string Name, string Sku, string Barcode, decimal PurchasePrice);
public sealed record PurchaseOrderView(Guid Id, string Number, Guid WarehouseId, string WarehouseName,
    Guid ContractorId, string ContractorName, string Status, DateTime CreatedOnUtc,
    DateTime? ApprovedOnUtc, DateTime? ReceivedOnUtc, string? InvoiceNumber,
    string? PaperDocumentNumber, string? Notes, decimal TotalValue, IReadOnlyList<PurchaseOrderItemView> Items);
public sealed record PurchaseOrderItemView(Guid Id, Guid ProductId, string ProductName, string Sku,
    string Barcode, decimal Quantity, decimal UnitPrice, decimal TotalPrice);
public sealed record CreatePurchaseOrderRequest(Guid ContractorId, IReadOnlyList<CreatePurchaseOrderItem> Items, string? Notes);
public sealed record CreatePurchaseOrderItem(Guid ProductId, decimal Quantity, decimal? UnitPrice);
public sealed record ApprovePurchaseOrderRequest(string InvoiceNumber, string PaperDocumentNumber, string? Notes);
public sealed record ReceivePurchaseOrderRequest(IReadOnlyCollection<Guid> CheckedItemIds, string? Notes);
