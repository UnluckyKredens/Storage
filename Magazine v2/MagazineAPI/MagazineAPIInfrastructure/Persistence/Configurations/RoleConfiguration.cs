using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .ValueGeneratedOnAdd();

        builder.Property(role => role.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.HasData(
            new Role
            {
                Id = RoleIds.Administrator,
                Name = RoleNames.Administrator
            },
            new Role
            {
                Id = RoleIds.Manager,
                Name = RoleNames.Manager
            },
            new Role
            {
                Id = RoleIds.User,
                Name = RoleNames.User
            });
    }
}
