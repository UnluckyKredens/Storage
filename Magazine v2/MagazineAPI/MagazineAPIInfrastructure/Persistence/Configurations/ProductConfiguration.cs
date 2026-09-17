using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.ProductId);

        builder.Property(product => product.ProductId)
            .HasColumnName("ProductID")
            .ValueGeneratedOnAdd();

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Sku)
            .HasColumnName("SKU")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(product => product.Barcode)
            .HasMaxLength(100);

        builder.Property(product => product.Description)
            .HasMaxLength(1000);

        builder.Property(product => product.UnitOfMeasureId)
            .HasColumnName("UnitOfMeasureID");

        builder.Property(product => product.CategoryId)
            .HasColumnName("CategoryID");

        builder.Property(product => product.PurchasePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.SalePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.IsActive)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Products_Prices",
                "[PurchasePrice] >= 0 AND [SalePrice] >= 0");
        });

        builder.HasOne(product => product.UnitOfMeasure)
            .WithMany(unit => unit.Products)
            .HasForeignKey(product => product.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
