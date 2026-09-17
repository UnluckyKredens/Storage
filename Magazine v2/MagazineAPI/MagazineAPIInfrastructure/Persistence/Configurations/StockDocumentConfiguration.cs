using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public sealed class StockDocumentConfiguration : IEntityTypeConfiguration<StockDocument>
{
    public void Configure(EntityTypeBuilder<StockDocument> builder)
    {
        builder.ToTable("StockDocuments", table =>
        {
            table.HasCheckConstraint(
                "CK_StockDocuments_Type",
                "[Type] IN (1, 2)");
            table.HasCheckConstraint(
                "CK_StockDocuments_Status",
                "[Status] IN (1, 2, 3)");
        });
        builder.HasKey(document => document.Id);

        builder.Property(document => document.Number)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(document => document.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(document => document.Number)
            .IsUnique();

        builder.HasOne(document => document.Warehouse)
            .WithMany()
            .HasForeignKey(document => document.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(document => document.Contractor)
            .WithMany()
            .HasForeignKey(document => document.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(document => document.DestinationWarehouse)
            .WithMany()
            .HasForeignKey(document => document.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(document => document.CreatedByUser)
            .WithMany()
            .HasForeignKey(document => document.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(document => document.ApprovedByUser)
            .WithMany()
            .HasForeignKey(document => document.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(document => document.ReceivedByUser)
            .WithMany()
            .HasForeignKey(document => document.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
