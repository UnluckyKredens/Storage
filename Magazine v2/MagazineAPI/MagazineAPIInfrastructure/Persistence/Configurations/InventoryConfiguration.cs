using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.Property(item => item.RowVersion).IsRowVersion();
        builder.ToTable("Inventory");

        builder.HasKey(inventory => inventory.Id);

        builder.Property(inventory => inventory.Id)
            .ValueGeneratedOnAdd();

        builder.Property(inventory => inventory.ProductId);

        builder.Property(inventory => inventory.LocationId);

        builder.Property(inventory => inventory.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(inventory => inventory.ReservedQuantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(inventory => inventory.AvailableQuantity)
            .HasPrecision(18, 3)
            .HasComputedColumnSql("[Quantity] - [ReservedQuantity]", stored: true);

        builder.HasIndex(inventory => new { inventory.ProductId, inventory.LocationId })
            .IsUnique();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Inventory_QuantityValues",
                "[Quantity] >= 0 AND [ReservedQuantity] >= 0 AND [ReservedQuantity] <= [Quantity]");
        });

        builder.HasOne(inventory => inventory.Product)
            .WithMany(product => product.Inventories)
            .HasForeignKey(inventory => inventory.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inventory => inventory.Location)
            .WithMany(location => location.Inventories)
            .HasForeignKey(inventory => inventory.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
