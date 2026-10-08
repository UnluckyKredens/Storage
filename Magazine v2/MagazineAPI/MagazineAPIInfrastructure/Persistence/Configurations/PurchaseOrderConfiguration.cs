using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(order => order.PurchaseOrderId);

        builder.Property(order => order.PurchaseOrderId)
            .ValueGeneratedOnAdd();

        builder.Property(order => order.Number)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.InvoiceNumber)
            .HasMaxLength(100);

        builder.Property(order => order.PaperDocumentNumber)
            .HasMaxLength(100);

        builder.Property(order => order.Notes)
            .HasMaxLength(1000);

        builder.Property(order => order.CreatedOnUtc)
            .IsRequired();

        builder.HasIndex(order => order.Number)
            .IsUnique();

        builder.HasIndex(order => new { order.Status, order.CreatedOnUtc });

        builder.HasIndex(order => new { order.WarehouseId, order.CreatedOnUtc });

        builder.HasOne(order => order.Warehouse)
            .WithMany(warehouse => warehouse.PurchaseOrders)
            .HasForeignKey(order => order.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.Contractor)
            .WithMany(contractor => contractor.PurchaseOrders)
            .HasForeignKey(order => order.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.CreatedByUser)
            .WithMany(user => user.CreatedPurchaseOrders)
            .HasForeignKey(order => order.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.ApprovedByUser)
            .WithMany(user => user.ApprovedPurchaseOrders)
            .HasForeignKey(order => order.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.ReceivedByUser)
            .WithMany(user => user.ReceivedPurchaseOrders)
            .HasForeignKey(order => order.ReceivedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_PurchaseOrders_Status",
                $"[Status] IN ({(int)PurchaseOrderStatus.PendingApproval}, {(int)PurchaseOrderStatus.Approved}, {(int)PurchaseOrderStatus.Received})");
        });
    }
}
