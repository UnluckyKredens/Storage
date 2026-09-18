using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");
        builder.HasKey(reservation => reservation.StockReservationId);
        builder.Property(reservation => reservation.Quantity).HasPrecision(18, 3).IsRequired();
        builder.Property(reservation => reservation.ReleasedQuantity).HasPrecision(18, 3).IsRequired();
        builder.Property(reservation => reservation.Status).HasConversion<int>().IsRequired();
        builder.Property(reservation => reservation.SourceType).HasMaxLength(50).IsRequired();
        builder.Property(reservation => reservation.SourceNumber).HasMaxLength(50);
        builder.HasIndex(reservation => new { reservation.SourceType, reservation.SourceId });
        builder.HasIndex(reservation => new { reservation.WarehouseId, reservation.ProductId, reservation.Status });

        builder.HasOne(reservation => reservation.Warehouse)
            .WithMany()
            .HasForeignKey(reservation => reservation.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(reservation => reservation.Location)
            .WithMany()
            .HasForeignKey(reservation => reservation.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(reservation => reservation.Product)
            .WithMany()
            .HasForeignKey(reservation => reservation.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(reservation => reservation.Inventory)
            .WithMany()
            .HasForeignKey(reservation => reservation.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(reservation => reservation.CreatedByUser)
            .WithMany()
            .HasForeignKey(reservation => reservation.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_StockReservations_Status",
                $"[Status] IN ({(int)StockReservationStatus.Active}, {(int)StockReservationStatus.Released}, {(int)StockReservationStatus.Consumed}, {(int)StockReservationStatus.Cancelled})");
            tableBuilder.HasCheckConstraint(
                "CK_StockReservations_QuantityValues",
                "[Quantity] > 0 AND [ReleasedQuantity] >= 0 AND [ReleasedQuantity] <= [Quantity]");
        });
    }
}
