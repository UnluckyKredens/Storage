using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Authorization;
using MagazineAPIDomain.Entities;
using MagazineAPInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPI.Startup;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();

        var options = configuration
            .GetSection(InitialAdminOptions.SectionName)
            .Get<InitialAdminOptions>();

        if (options is null || !options.Enabled)
        {
            return;
        }

        var administratorExists = await dbContext.Users
            .AnyAsync(user => user.RoleId == RoleIds.Administrator);

        if (administratorExists)
        {
            return;
        }

        Validate(options);

        var passwordHashingService = scope.ServiceProvider
            .GetRequiredService<IPasswordHashingService>();

        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Login = options.Login.Trim(),
            FirstName = options.FirstName.Trim(),
            LastName = options.LastName.Trim(),
            Email = options.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHashingService.HashPassword(options.Password),
            RoleId = RoleIds.Administrator,
            WarehouseId = null
        });

        await dbContext.SaveChangesAsync();
    }

    private static void Validate(InitialAdminOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Login)
            || string.IsNullOrWhiteSpace(options.FirstName)
            || string.IsNullOrWhiteSpace(options.LastName)
            || string.IsNullOrWhiteSpace(options.Email)
            || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Konfiguracja InitialAdmin musi zawierać komplet danych administratora.");
        }

        if (options.Password.Length < 8)
        {
            throw new InvalidOperationException(
                "Hasło InitialAdmin musi mieć co najmniej 8 znaków.");
        }
    }
}
