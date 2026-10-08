using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class StorefrontOrderItemConfiguration : IEntityTypeConfiguration<StorefrontOrderItem>
{
    public void Configure(EntityTypeBuilder<StorefrontOrderItem> builder)
    {
        builder.ToTable("StorefrontOrderItems");

        builder.HasKey(item => item.StorefrontOrderItemId);

        builder.Property(item => item.StorefrontOrderItemId)
            .ValueGeneratedOnAdd();

        builder.Property(item => item.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(item => item.StorefrontOrder)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.StorefrontOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Product)
            .WithMany(product => product.StorefrontOrderItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StorefrontOrderItems_Quantity", "[Quantity] > 0");
            tableBuilder.HasCheckConstraint("CK_StorefrontOrderItems_UnitPrice", "[UnitPrice] >= 0");
        });
    }
}
