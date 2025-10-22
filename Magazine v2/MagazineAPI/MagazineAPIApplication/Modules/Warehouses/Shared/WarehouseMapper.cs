using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Modules.Warehouses;

internal static class WarehouseMapper
{
    public static WarehouseView ToView(Warehouse item) => new(
            item.Id,
            item.Name,
            item.Address,
            item.Description);
}
