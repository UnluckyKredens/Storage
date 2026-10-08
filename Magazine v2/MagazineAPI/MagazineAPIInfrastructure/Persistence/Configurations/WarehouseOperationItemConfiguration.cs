using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class WarehouseOperationItemConfiguration : IEntityTypeConfiguration<WarehouseOperationItem>
{
    public void Configure(EntityTypeBuilder<WarehouseOperationItem> builder)
    {
        builder.ToTable("WarehouseOperationItems");
        builder.HasKey(item => item.WarehouseOperationItemId);
        builder.Property(item => item.Quantity).HasPrecision(18, 3).IsRequired();
        builder.Property(item => item.TargetQuantity).HasPrecision(18, 3);
        builder.HasIndex(item => item.WarehouseOperationId);

        builder.HasOne(item => item.WarehouseOperation)
            .WithMany(operation => operation.Items)
            .HasForeignKey(item => item.WarehouseOperationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.SourceLocation)
            .WithMany()
            .HasForeignKey(item => item.SourceLocationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.DestinationLocation)
            .WithMany()
            .HasForeignKey(item => item.DestinationLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_WarehouseOperationItems_Quantity",
                "[Quantity] >= 0 AND ([TargetQuantity] IS NULL OR [TargetQuantity] >= 0)");
        });
    }
}
