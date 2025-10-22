using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class StorefrontOrderAllocationConfiguration : IEntityTypeConfiguration<StorefrontOrderAllocation>
{
    public void Configure(EntityTypeBuilder<StorefrontOrderAllocation> builder)
    {
        builder.ToTable("StorefrontOrderAllocations");

        builder.HasKey(allocation => allocation.Id);

        builder.Property(allocation => allocation.Id)
            .ValueGeneratedOnAdd();

        builder.Property(allocation => allocation.WarehouseId);

        builder.Property(allocation => allocation.LocationId);

        builder.Property(allocation => allocation.InventoryId);

        builder.Property(allocation => allocation.ProductId);

        builder.Property(allocation => allocation.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(allocation => allocation.ConsumedQuantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.HasIndex(allocation => allocation.StorefrontOrderItemId);
        builder.HasIndex(allocation => new { allocation.WarehouseId, allocation.ProductId });

        builder.HasOne(allocation => allocation.StorefrontOrderItem)
            .WithMany(item => item.Allocations)
            .HasForeignKey(allocation => allocation.StorefrontOrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(allocation => allocation.Warehouse)
            .WithMany()
            .HasForeignKey(allocation => allocation.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(allocation => allocation.Location)
            .WithMany()
            .HasForeignKey(allocation => allocation.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(allocation => allocation.Inventory)
            .WithMany()
            .HasForeignKey(allocation => allocation.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(allocation => allocation.Product)
            .WithMany()
            .HasForeignKey(allocation => allocation.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_StorefrontOrderAllocations_QuantityValues",
                "[Quantity] > 0 AND [ConsumedQuantity] >= 0 AND [ConsumedQuantity] <= [Quantity]");
        });
    }
}
