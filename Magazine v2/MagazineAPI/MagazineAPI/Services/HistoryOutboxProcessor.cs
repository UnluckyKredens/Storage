using System.Text.Json;
using MagazineAPIDomain.Entities;
using MagazineAPIDomain.Entities.History;
using MagazineAPInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MagazineAPI.Services;

public sealed class HistoryOutboxProcessor(IServiceScopeFactory scopes, ILogger<HistoryOutboxProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { logger.LogWarning(ex, "Historia jest chwilowo niedostępna; wiadomości pozostają w outboxie."); }
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopes.CreateAsyncScope();
        var source = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var history = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var messages = await source.HistoryOutboxMessages.Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.CreatedOnUtc).Take(100).ToArrayAsync(cancellationToken);
        foreach (var message in messages)
        {
            if (message.EntityType == nameof(InventoryHistory))
            {
                if (!await history.InventoryHistories.AnyAsync(item => item.Id == message.Id, cancellationToken))
                    history.InventoryHistories.Add(JsonSerializer.Deserialize<InventoryHistory>(message.Payload)!);
            }
            else if (message.EntityType == nameof(ShipmentHistory))
            {
                if (!await history.ShipmentHistories.AnyAsync(item => item.Id == message.Id, cancellationToken))
                    history.ShipmentHistories.Add(JsonSerializer.Deserialize<ShipmentHistory>(message.Payload)!);
            }
            else throw new InvalidOperationException($"Nieobsługiwany typ outboxa: {message.EntityType}.");
            // Crash between these saves is safe: the history primary key deduplicates delivery.
            await history.SaveChangesAsync(cancellationToken);
            message.ProcessedOnUtc = DateTime.UtcNow;
            await source.SaveChangesAsync(cancellationToken);
            history.ChangeTracker.Clear();
        }
        return messages.Length;
    }
}
