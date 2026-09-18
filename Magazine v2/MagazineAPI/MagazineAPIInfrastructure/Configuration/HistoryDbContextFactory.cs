using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MagazineAPInfrastructure.Persistence;

public class HistoryDbContextFactory: IDesignTimeDbContextFactory<HistoryDbContext>
{
    public HistoryDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("MagazineAPI/appsettings.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

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