using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MagazineAPInfrastructure.Persistence;

public class HistoryDbContextFactory: IDesignTimeDbContextFactory<HistoryDbContext>
{
    public HistoryDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Load(args);

        var connectionString = configuration.GetConnectionString("HistoryConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Brak ConnectionStrings:HistoryConnection w appsettings.json.");
        }

        var options = new DbContextOptionsBuilder<HistoryDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new HistoryDbContext(options);
    }
}