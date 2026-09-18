using MagazineWarehouseBot.Api;
using MagazineWarehouseBot.Models;
using MagazineWarehouseBot.Simulation;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5260");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<MagazineApiOptions>(
    builder.Configuration.GetSection(MagazineApiOptions.SectionName));
builder.Services.Configure<WarehouseBotOptions>(
    builder.Configuration.GetSection(WarehouseBotOptions.SectionName));

builder.Services.AddSingleton<BotActionLog>();
builder.Services.AddSingleton<BotControlState>();
builder.Services.AddHttpClient<MagazineApiClient>((services, client) =>
{
    var options = services.GetRequiredService<IOptions<MagazineApiOptions>>().Value;
    client.BaseAddress = new Uri(options.NormalizedBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHostedService<WarehouseBotService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/bot/snapshot", (BotControlState state, BotActionLog log) =>
{
    return Results.Ok(new BotSnapshot(
        state.IsRunning,
        state.GetActors().Select(BotActorSnapshot.From).ToArray(),
        log.GetRecentEvents(),
        log.GetStats()));
});

app.MapPost("/api/bot/accounts", (ConfigureAccountsRequest request, BotControlState state, BotActionLog log) =>
{
    state.ConfigureAccounts(request.Accounts);
    log.Info("System", "Konfiguracja", "Zaktualizowano konta botow.");
    return Results.Ok();
});

app.MapPost("/api/bot/start", (BotControlState state, BotActionLog log) =>
{
    state.Start();
    log.Info("System", "Start", "Symulator zostal uruchomiony.");
    return Results.Ok();
});

app.MapPost("/api/bot/stop", (BotControlState state, BotActionLog log) =>
{
    state.Stop();
    log.Info("System", "Stop", "Symulator zostal zatrzymany.");
    return Results.Ok();
});

app.Run();
