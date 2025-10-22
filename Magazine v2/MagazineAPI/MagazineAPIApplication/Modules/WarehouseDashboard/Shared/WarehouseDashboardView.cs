namespace MagazineAPIApplication.Modules.WarehouseDashboard;

public sealed record WarehouseDashboardView(
    int ProductCount,
    int InventoryItemCount,
    decimal TotalQuantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    int ActiveReservationCount,
    int PendingShipmentCount,
    int SentShipmentCount,
    int InTransitShipmentCount,
    IReadOnlyList<WarehouseDashboardAlertView> Alerts);

public sealed record WarehouseDashboardAlertView(
    string Type,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductName,
    decimal AvailableQuantity,
    decimal MinimumQuantity,
    decimal? OptimumQuantity,
    string Message);
