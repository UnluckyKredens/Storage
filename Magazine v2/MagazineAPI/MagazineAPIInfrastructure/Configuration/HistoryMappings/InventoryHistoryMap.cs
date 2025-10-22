using MagazineAPIDomain.Entities.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.HistoryMappings;

public sealed class InventoryHistoryMap
    : IEntityTypeConfiguration<InventoryHistory>
{
    public void Configure(EntityTypeBuilder<InventoryHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 4);

        builder.Property(x => x.ReservedQuantity)
            .HasPrecision(18, 4);

        builder.Property(x => x.QuantityChange)
            .HasPrecision(18, 4);

        builder.Property(x => x.AggregateId);
        builder.Property(x => x.ItemId);
        builder.Property(x => x.LocationId);
        builder.Property(x => x.ProductId);
        builder.Property(x => x.ReservedQuantity);
        builder.Property(x => x.CreatedOn);
    }
}