using MagazineAPIDomain.Entities;

namespace MagazineAPIApplication.Modules.UnitsOfMeasure;

internal static class UnitOfMeasureMapper
{
    public static UnitOfMeasureView ToView(UnitOfMeasure item) => new(
            item.UnitOfMeasureId,
            item.Name,
            item.Symbol);
}
