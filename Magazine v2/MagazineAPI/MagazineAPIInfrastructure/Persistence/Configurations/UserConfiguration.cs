using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("UserID")
            .ValueGeneratedOnAdd();

        builder.Property(user => user.Login)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.RoleId)
            .HasColumnName("RoleID")
            .IsRequired();

        builder.Property(user => user.WarehouseId)
            .HasColumnName("WarehouseID");

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasIndex(user => user.Login)
            .IsUnique();

        builder.HasOne(user => user.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(user => user.Warehouse)
            .WithMany(warehouse => warehouse.Users)
            .HasForeignKey(user => user.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Users_WarehouseAssignment",
            $"[RoleID] = '{MagazineAPIDomain.Authorization.RoleIds.Administrator}' OR [WarehouseID] IS NOT NULL"));

    }
}
