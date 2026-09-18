using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(shipment => shipment.ShipmentId);

        builder.Property(shipment => shipment.ShipmentId)
            .ValueGeneratedOnAdd();

        builder.Property(shipment => shipment.Number)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(shipment => shipment.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(shipment => shipment.CreatedOnUtc)
            .IsRequired();

        builder.HasIndex(shipment => shipment.Number)
            .IsUnique();

        builder.HasIndex(shipment => new { shipment.Status, shipment.CreatedOnUtc });

        builder.HasIndex(shipment => new { shipment.SourceWarehouseId, shipment.CreatedOnUtc });

        builder.HasIndex(shipment => new { shipment.DestinationWarehouseId, shipment.CreatedOnUtc });

        builder.HasOne(shipment => shipment.SourceWarehouse)
            .WithMany(warehouse => warehouse.SourceShipments)
            .HasForeignKey(shipment => shipment.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(shipment => shipment.DestinationWarehouse)
            .WithMany(warehouse => warehouse.DestinationShipments)
            .HasForeignKey(shipment => shipment.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(shipment => shipment.CreatedByUser)
            .WithMany(user => user.CreatedShipments)
            .HasForeignKey(shipment => shipment.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(shipment => shipment.ApprovedByUser)
            .WithMany(user => user.ApprovedShipments)
            .HasForeignKey(shipment => shipment.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(shipment => shipment.ReceivedByUser)
            .WithMany(user => user.ReceivedShipments)
            .HasForeignKey(shipment => shipment.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Shipments_Status",
                $"[Status] IN ({(int)ShipmentStatus.PendingApproval}, {(int)ShipmentStatus.Sent}, {(int)ShipmentStatus.InTransit}, {(int)ShipmentStatus.Received})");
            tableBuilder.HasCheckConstraint(
                "CK_Shipments_DifferentWarehouses",
                "[SourceWarehouseId] <> [DestinationWarehouseId]");
        });
    }
}
