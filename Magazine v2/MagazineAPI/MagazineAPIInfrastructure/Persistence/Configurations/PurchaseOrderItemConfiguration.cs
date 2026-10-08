using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");

        builder.HasKey(item => item.PurchaseOrderItemId);

        builder.Property(item => item.PurchaseOrderItemId)
            .ValueGeneratedOnAdd();

        builder.Property(item => item.Barcode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(item => new { item.PurchaseOrderId, item.ProductId })
            .IsUnique();

        builder.HasOne(item => item.PurchaseOrder)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Product)
            .WithMany(product => product.PurchaseOrderItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_PurchaseOrderItems_Quantity", "[Quantity] > 0");
            tableBuilder.HasCheckConstraint("CK_PurchaseOrderItems_UnitPrice", "[UnitPrice] >= 0");
        });
    }
}
