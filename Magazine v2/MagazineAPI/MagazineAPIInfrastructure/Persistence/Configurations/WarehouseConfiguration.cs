using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(warehouse => warehouse.WarehouseId);

        builder.Property(warehouse => warehouse.WarehouseId)
            .HasColumnName("WarehouseID")
            .ValueGeneratedOnAdd();

        builder.Property(warehouse => warehouse.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(warehouse => warehouse.Address)
            .HasMaxLength(500);

        builder.Property(warehouse => warehouse.Description)
            .HasMaxLength(1000);
    }
}
