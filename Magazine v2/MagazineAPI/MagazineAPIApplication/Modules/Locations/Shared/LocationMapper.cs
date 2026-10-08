using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Modules.Locations;

internal static class LocationMapper
{
    public static LocationView ToView(Location item, string warehouseName) => new(
            item.LocationId,
            item.WarehouseId,
            warehouseName,
            item.LocationCode,
            item.Description);
}
