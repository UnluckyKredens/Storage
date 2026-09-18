using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(log => log.AuditLogId);

        builder.Property(log => log.AuditLogId)
            .ValueGeneratedOnAdd();

        builder.Property(log => log.CreatedOnUtc)
            .IsRequired();

        builder.Property(log => log.Action)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(log => log.EntityName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(log => log.EntityId)
            .HasMaxLength(80);

        builder.Property(log => log.Summary)
            .HasMaxLength(1000);

        builder.Property(log => log.BeforeValuesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(log => log.AfterValuesJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(log => log.CreatedOnUtc);
        builder.HasIndex(log => new { log.EntityName, log.EntityId });
        builder.HasIndex(log => log.UserId);

        builder.HasOne(log => log.User)
            .WithMany()
            .HasForeignKey(log => log.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
