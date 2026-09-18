using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class WarehouseOperationConfiguration : IEntityTypeConfiguration<WarehouseOperation>
{
    public void Configure(EntityTypeBuilder<WarehouseOperation> builder)
    {
        builder.ToTable("WarehouseOperations");
        builder.HasKey(operation => operation.WarehouseOperationId);
        builder.Property(operation => operation.Number).HasMaxLength(50).IsRequired();
        builder.Property(operation => operation.Type).HasConversion<int>().IsRequired();
        builder.Property(operation => operation.Status).HasConversion<int>().IsRequired();
        builder.Property(operation => operation.Notes).HasMaxLength(1000);
        builder.HasIndex(operation => operation.Number).IsUnique();
        builder.HasIndex(operation => new { operation.WarehouseId, operation.CompletedOnUtc });

        builder.HasOne(operation => operation.Warehouse)
            .WithMany()
            .HasForeignKey(operation => operation.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(operation => operation.CreatedByUser)
            .WithMany()
            .HasForeignKey(operation => operation.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(operation => operation.CompletedByUser)
            .WithMany()
            .HasForeignKey(operation => operation.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_WarehouseOperations_Type",
                $"[Type] IN ({(int)WarehouseOperationType.InternalReceipt}, {(int)WarehouseOperationType.InternalIssue}, {(int)WarehouseOperationType.InternalTransfer}, {(int)WarehouseOperationType.Correction}, {(int)WarehouseOperationType.InventoryCount})");
            tableBuilder.HasCheckConstraint(
                "CK_WarehouseOperations_Status",
                $"[Status] IN ({(int)WarehouseOperationStatus.Completed}, {(int)WarehouseOperationStatus.Cancelled})");
        });
    }
}
