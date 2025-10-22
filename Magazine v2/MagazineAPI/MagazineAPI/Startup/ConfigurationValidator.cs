using System.Text;
using MagazineAPIApplication.Common.Security;

namespace MagazineAPI.Startup;

public static class ConfigurationValidator
{
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
        if (jwt is null || Encoding.UTF8.GetByteCount(jwt.SecretKey) < 32 ||
            string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience) ||
            jwt.ExpirationMinutes is < 1 or > 1440 ||
            (!environment.IsDevelopment() && jwt.SecretKey.Contains("Development", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Podaj niezależny sekret JWT (minimum 32 bajty), issuer, audience i czas ważności 1–1440 minut.");
        foreach (var name in new[] { "DefaultConnection", "HistoryConnection" })
            if (string.IsNullOrWhiteSpace(configuration.GetConnectionString(name)))
                throw new InvalidOperationException($"Brak ConnectionStrings:{name}. Dla lokalnych testów uruchom scripts/setup-local.py.");
    }
}
