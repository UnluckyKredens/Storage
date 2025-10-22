namespace MagazineAPIApplication.Modules.WarehouseDashboard;

public sealed record WarehouseFlowStatsView(
    int Days,
    DateTime FromUtc,
    DateTime ToUtc,
    decimal InboundQuantity,
    decimal OutboundQuantity,
    decimal NetChange,
    int MovementCount,
    IReadOnlyList<WarehouseFlowDayView> Daily,
    IReadOnlyList<WarehouseFlowProductView> TopProducts,
    WarehouseShipmentFlowView Shipments);

public sealed record WarehouseFlowDayView(DateOnly Date, decimal Inbound, decimal Outbound);

public sealed record WarehouseFlowProductView(Guid ProductId, string ProductName, decimal Inbound, decimal Outbound);

public sealed record WarehouseShipmentFlowView(
    int Created,
    int Requested,
    int Approved,
    int InTransit,
    int Received,
    int Cancelled,
    double? AverageApprovalHours,
    double? AverageDeliveryHours);
