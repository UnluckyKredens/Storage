using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public sealed class StockDocumentItemConfiguration : IEntityTypeConfiguration<StockDocumentItem>
{
    public void Configure(EntityTypeBuilder<StockDocumentItem> builder)
    {
        builder.ToTable("StockDocumentItems", table =>
            table.HasCheckConstraint("CK_StockDocumentItems_Quantity", "[Quantity] > 0"));
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Quantity)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.HasIndex(item => new
        {
            item.StockDocumentId,
            item.ProductId,
            item.LocationId
        }).IsUnique();

        builder.HasOne(item => item.StockDocument)
            .WithMany(document => document.Items)
            .HasForeignKey(item => item.StockDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Product)
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.Location)
            .WithMany()
            .HasForeignKey(item => item.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
