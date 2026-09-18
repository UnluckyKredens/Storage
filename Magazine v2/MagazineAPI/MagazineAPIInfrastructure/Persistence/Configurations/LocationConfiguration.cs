using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(location => location.LocationId);

        builder.Property(location => location.LocationId)
            .HasColumnName("LocationID")
            .ValueGeneratedOnAdd();

        builder.Property(location => location.WarehouseId)
            .HasColumnName("WarehouseID");

        builder.Property(location => location.LocationCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(location => location.Description)
            .HasMaxLength(500);

        builder.HasIndex(location => new { location.WarehouseId, location.LocationCode })
            .IsUnique();

        builder.HasOne(location => location.Warehouse)
            .WithMany(warehouse => warehouse.Locations)
            .HasForeignKey(location => location.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
