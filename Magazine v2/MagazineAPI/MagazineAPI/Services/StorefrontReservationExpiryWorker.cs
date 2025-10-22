using MagazineAPIDomain.Enums;
using MagazineAPIDomain.Repositories;
using MagazineAPInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPI.Services;

public sealed class StorefrontReservationExpiryWorker(IServiceScopeFactory scopes,
    ILogger<StorefrontReservationExpiryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ExpireAsync(stoppingToken); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { logger.LogWarning(ex, "Nie udało się zwolnić wygasłych rezerwacji; operacja zostanie ponowiona."); }
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    public async Task ExpireAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var service = scope.ServiceProvider.GetRequiredService<StorefrontOrderService>();
        var now = DateTime.UtcNow;
        var ids = await db.StorefrontOrders.AsNoTracking().Where(order => order.ExpiresOnUtc <= now &&
            (order.Status == StorefrontOrderStatus.New || order.Status == StorefrontOrderStatus.Accepted))
            .OrderBy(order => order.ExpiresOnUtc).Select(order => order.Id).Take(100).ToArrayAsync(ct);
        foreach (var id in ids)
        {
            await unit.ExecuteInTransactionAsync(async token =>
            {
                var order = await service.OrdersWithAllocations().SingleAsync(item => item.Id == id, token);
                if (order.ExpiresOnUtc <= DateTime.UtcNow)
                    await service.ReleaseAsync(order, "Upłynął termin rezerwacji zamówienia.", token);
            }, ct);
        }
    }
}
