using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MagazineAPInfrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rolePermission => new
        {
            rolePermission.RoleId,
            rolePermission.PermissionId
        });

        builder.HasOne(rolePermission => rolePermission.Role)
            .WithMany(role => role.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rolePermission => rolePermission.Permission)
            .WithMany(permission => permission.RolePermissions)
            .HasForeignKey(rolePermission => rolePermission.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.ProductsRead },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.ProductsManage },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.InventoryRead },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.InventoryManage },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.WarehousesRead },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.ContractorsRead },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.ShipmentsRead },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.ShipmentsCreate },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.ShipmentsApprove },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.PurchaseOrdersRead },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.PurchaseOrdersCreate },
            new RolePermission { RoleId = RoleIds.Manager, PermissionId = PermissionIds.PurchaseOrdersApprove },
            new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.ProductsRead },
            new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.InventoryRead },
            new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.ShipmentsRead },
            new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.ShipmentsCreate },
            new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.PurchaseOrdersRead },
            new RolePermission { RoleId = RoleIds.User, PermissionId = PermissionIds.PurchaseOrdersCreate });
    }
}
