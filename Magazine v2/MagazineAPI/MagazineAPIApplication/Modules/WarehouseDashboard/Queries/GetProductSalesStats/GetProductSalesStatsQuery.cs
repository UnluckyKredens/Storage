using Mediator;

namespace MagazineAPIApplication.Modules.WarehouseDashboard;

public sealed record GetProductSalesStatsQuery(int Days) : IQuery<ProductSalesStatsView>;
