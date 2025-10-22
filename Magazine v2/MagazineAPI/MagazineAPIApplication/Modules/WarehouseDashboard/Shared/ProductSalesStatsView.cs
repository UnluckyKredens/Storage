namespace MagazineAPIApplication.Modules.WarehouseDashboard;

public sealed record ProductSalesView(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal SoldQuantity,
    decimal Revenue,
    int OrderCount,
    decimal StockQuantity);

public sealed record ProductSalesStatsView(
    int Days,
    DateTime FromUtc,
    DateTime ToUtc,
    int OrderCount,
    decimal SoldQuantity,
    decimal Revenue,
    IReadOnlyList<ProductSalesView> BestSellers,
    IReadOnlyList<ProductSalesView> WorstSellers);
