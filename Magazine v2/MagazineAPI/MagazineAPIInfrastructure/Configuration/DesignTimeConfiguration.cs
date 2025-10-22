using Microsoft.Extensions.Configuration;

namespace MagazineAPInfrastructure.Persistence;

internal static class DesignTimeConfiguration
{
    public static IConfiguration Load(string[] args)
    {
        var candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var candidate in candidates)
        {
            for (var directory = new DirectoryInfo(candidate); directory is not null; directory = directory.Parent)
            {
                foreach (var relative in new[] { "MagazineAPI/MagazineAPI", "MagazineAPI", "." })
                {
                    var root = Path.GetFullPath(Path.Combine(directory.FullName, relative));
                    if (!File.Exists(Path.Combine(root, "appsettings.json")) ||
                        !File.Exists(Path.Combine(root, "MagazineAPI.csproj"))) continue;
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                    var builder = new ConfigurationBuilder().SetBasePath(root)
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{environment}.json", optional: true);
                    if (environment == "Development") builder.AddJsonFile("appsettings.Local.json", optional: true);
                    return builder.AddEnvironmentVariables().AddCommandLine(args).Build();
                }
            }
        }
        throw new InvalidOperationException("Nie znaleziono katalogu konfiguracji MagazineAPI.");
    }
}
