using System.Collections.Concurrent;
using MagazineWarehouseBot.Models;
using Microsoft.Extensions.Options;

namespace MagazineWarehouseBot.Simulation;

public sealed class BotActionLog(IOptions<WarehouseBotOptions> options)
{
    private readonly ConcurrentQueue<BotEvent> _events = new();
    private readonly ConcurrentDictionary<string, int> _stats = new(StringComparer.OrdinalIgnoreCase);
    private long _nextId;

    public void Info(string actor, string action, string message) =>
        Add(actor, action, message, true);

    public void Warn(string actor, string action, string message) =>
        Add(actor, action, message, false);

    public void Add(string actor, string action, string message, bool success)
    {
        var item = new BotEvent(
            Interlocked.Increment(ref _nextId),
            DateTimeOffset.Now,
            actor,
            action,
            message,
            success);

        _events.Enqueue(item);
        _stats.AddOrUpdate(action, 1, (_, count) => count + 1);

        var maxEvents = Math.Max(options.Value.MaxRecentEvents, 50);
        while (_events.Count > maxEvents && _events.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<BotEvent> GetRecentEvents() =>
        _events.OrderByDescending(x => x.Id).ToArray();

    public IReadOnlyDictionary<string, int> GetStats() =>
        _stats.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
}

public sealed record BotEvent(
    long Id,
    DateTimeOffset At,
    string Actor,
    string Action,
    string Message,
    bool Success);

public sealed record BotSnapshot(
    bool IsRunning,
    IReadOnlyList<BotActorSnapshot> Actors,
    IReadOnlyList<BotEvent> Events,
    IReadOnlyDictionary<string, int> Stats);
