using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.ToTable("ShipmentItems");

        builder.HasKey(item => item.ShipmentItemId);

        builder.Property(item => item.ShipmentItemId)
            .ValueGeneratedOnAdd();

        builder.Property(item => item.Barcode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.HasIndex(item => new { item.ShipmentId, item.ProductId })
            .IsUnique();

        builder.HasOne(item => item.Shipment)
            .WithMany(shipment => shipment.Items)
            .HasForeignKey(item => item.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Product)
            .WithMany(product => product.ShipmentItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_ShipmentItems_Quantity",
                "[Quantity] > 0");
        });
    }
}
