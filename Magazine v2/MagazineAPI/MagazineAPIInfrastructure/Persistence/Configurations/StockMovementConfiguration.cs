using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(movement => movement.StockMovementId);

        builder.Property(movement => movement.StockMovementId)
            .ValueGeneratedOnAdd();

        builder.Property(movement => movement.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(movement => movement.QuantityBefore)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(movement => movement.QuantityChange)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(movement => movement.QuantityAfter)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(movement => movement.ReservedQuantityBefore)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(movement => movement.ReservedQuantityChange)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(movement => movement.ReservedQuantityAfter)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(movement => movement.SourceType)
            .HasMaxLength(50);

        builder.Property(movement => movement.SourceNumber)
            .HasMaxLength(50);

        builder.Property(movement => movement.Notes)
            .HasMaxLength(1000);

        builder.Property(movement => movement.CreatedOnUtc)
            .IsRequired();

        builder.HasIndex(movement => movement.CreatedOnUtc);
        builder.HasIndex(movement => new { movement.WarehouseId, movement.CreatedOnUtc });
        builder.HasIndex(movement => new { movement.ProductId, movement.CreatedOnUtc });
        builder.HasIndex(movement => new { movement.SourceType, movement.SourceId });

        builder.HasOne(movement => movement.Warehouse)
            .WithMany()
            .HasForeignKey(movement => movement.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movement => movement.Location)
            .WithMany()
            .HasForeignKey(movement => movement.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movement => movement.Product)
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(movement => movement.Inventory)
            .WithMany()
            .HasForeignKey(movement => movement.InventoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(movement => movement.CreatedByUser)
            .WithMany()
            .HasForeignKey(movement => movement.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_StockMovements_Type",
                $"[Type] IN ({(int)StockMovementType.ManualAdjustment}, {(int)StockMovementType.TransferOut}, {(int)StockMovementType.TransferIn}, {(int)StockMovementType.Reservation}, {(int)StockMovementType.ReservationRelease}, {(int)StockMovementType.Correction}, {(int)StockMovementType.InventoryCount}, {(int)StockMovementType.InternalReceipt}, {(int)StockMovementType.InternalIssue}, {(int)StockMovementType.InternalTransferOut}, {(int)StockMovementType.InternalTransferIn}, {(int)StockMovementType.ExternalPurchaseReceipt})");
        });
    }
}
