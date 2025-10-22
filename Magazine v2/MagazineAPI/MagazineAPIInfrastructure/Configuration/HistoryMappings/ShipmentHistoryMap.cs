using MagazineAPIDomain.Entities.History;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.HistoryMappings;

public sealed class ShipmentHistoryMap : IEntityTypeConfiguration<ShipmentHistory>
{
    public void Configure(EntityTypeBuilder<ShipmentHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Details)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.ShipmentId);
        builder.Property(x => x.SourceWarehouseId);
        builder.Property(x => x.DestinationWarehouseId);
        builder.Property(x => x.UserId);
        builder.Property(x => x.CreatedOnUtc);
    }
}
