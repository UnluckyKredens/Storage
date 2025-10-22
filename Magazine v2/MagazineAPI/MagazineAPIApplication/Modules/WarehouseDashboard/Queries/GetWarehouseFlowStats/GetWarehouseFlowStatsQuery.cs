using Mediator;

namespace MagazineAPIApplication.Modules.WarehouseDashboard;

public sealed record GetWarehouseFlowStatsQuery(int Days) : IQuery<WarehouseFlowStatsView>;
