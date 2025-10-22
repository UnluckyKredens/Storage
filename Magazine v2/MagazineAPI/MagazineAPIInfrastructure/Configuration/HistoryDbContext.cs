using MagazineAPIDomain.Entities.History;
using MagazineAPInfrastructure.Persistence.HistoryMappings;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPInfrastructure.Persistence;

public sealed class HistoryDbContext : DbContext
{
    public HistoryDbContext(DbContextOptions<HistoryDbContext> options) : base(options)
    {}

    public DbSet<InventoryHistory> InventoryHistories { get; set; }
    public DbSet<ShipmentHistory> ShipmentHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new InventoryHistoryMap());
        modelBuilder.ApplyConfiguration(new ShipmentHistoryMap());
    }
}
