using MagazineAPIDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPInfrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<StorefrontOrder> StorefrontOrders => Set<StorefrontOrder>();
    public DbSet<StorefrontOrderItem> StorefrontOrderItems => Set<StorefrontOrderItem>();
    public DbSet<StorefrontOrderAllocation> StorefrontOrderAllocations => Set<StorefrontOrderAllocation>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DocumentNumberSequence> DocumentNumberSequences => Set<DocumentNumberSequence>();
    public DbSet<WarehouseOperation> WarehouseOperations => Set<WarehouseOperation>();
    public DbSet<WarehouseOperationItem> WarehouseOperationItems => Set<WarehouseOperationItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly,
            type => type.Namespace != "MagazineAPInfrastructure.Persistence.HistoryMappings");
        base.OnModelCreating(modelBuilder);
    }
}
