using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class StorefrontOrderConfiguration : IEntityTypeConfiguration<StorefrontOrder>
{
    public void Configure(EntityTypeBuilder<StorefrontOrder> builder)
    {
        builder.ToTable("StorefrontOrders");

        builder.HasKey(order => order.StorefrontOrderId);

        builder.Property(order => order.StorefrontOrderId)
            .ValueGeneratedOnAdd();

        builder.Property(order => order.Number)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(order => order.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(order => order.CustomerEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(order => order.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(order => order.DeliveryAddress)
            .HasMaxLength(500);

        builder.Property(order => order.Notes)
            .HasMaxLength(1000);

        builder.Property(order => order.AssignedWarehouseId)
            .HasColumnName("AssignedWarehouseID");

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.CreatedOnUtc)
            .IsRequired();

        builder.Property(order => order.AcceptedOnUtc);

        builder.Property(order => order.CompletedOnUtc);

        builder.HasIndex(order => order.Number)
            .IsUnique();

        builder.HasIndex(order => order.CreatedOnUtc);

        builder.HasIndex(order => new { order.AssignedWarehouseId, order.Status });

        builder.HasOne(order => order.AssignedWarehouse)
            .WithMany(warehouse => warehouse.StorefrontOrders)
            .HasForeignKey(order => order.AssignedWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_StorefrontOrders_Status",
                $"[Status] IN ({(int)StorefrontOrderStatus.New}, {(int)StorefrontOrderStatus.Accepted}, {(int)StorefrontOrderStatus.Completed}, {(int)StorefrontOrderStatus.Cancelled})");
        });
    }
}
