namespace MagazineAPIApplication.Modules.Inventories;

public sealed record InventoryPageDataView(
    IReadOnlyList<ProductOption> Products, IReadOnlyList<LocationOption> Locations);
