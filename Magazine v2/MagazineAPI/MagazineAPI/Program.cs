using MagazineAPInfrastructure.Persistence;
using MagazineAPInfrastructure.Persistence.Repositories;
using MagazineAPI.Authentication;
using MagazineAPI.Authorization;
using MagazineAPI.Services;
using MagazineAPI.Startup;
using MagazineAPIApplication;
using MagazineAPIApplication.Common.Security;
using MagazineAPIDomain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using MagazineAPIDomain.Entities.History;
using MagazineAPIApplication.Common.Exceptions;
using MagazineAPIApplication.Common.Interfaces;
using MagazineAPIEvent;
using Microsoft.AspNetCore.Diagnostics;
using System.Security.Claims;
using MagazineAPIDomain.Entities;
using Mediator;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true)
        .AddEnvironmentVariables().AddCommandLine(args);
ConfigurationValidator.Validate(builder.Configuration, builder.Environment);

// Add services to the container.

builder.Services.AddControllers(options => options.Filters.Add<RequestValidationFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableUtcDateTimeJsonConverter());
    });
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 256 * 1024);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Magazine API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Description = "Wpisz token JWT w formacie: Bearer {token}",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Scheme = "bearer",
        Type = SecuritySchemeType.Http
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", null, null),
            new List<string>()
        }
    });
});

builder.Services.AddMediator(opt =>
{
    opt.Assemblies = [typeof(ApplicationAssemblyReference), typeof(EventAssemblyReference)];
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CommandTransactionBehavior<,>));
builder.Services.AddHostedService<HistoryOutboxProcessor>();
builder.Services.AddHostedService<StorefrontReservationExpiryWorker>();
builder.Services.AddScoped<StorefrontOrderService>();

// Any origin may call the API. Access is protected by JWT bearer tokens (no cookies), not by the browser origin.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    foreach (var (name, limit) in new[] { ("login", 10), ("checkout", 20), ("tracking", 120) })
        options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
            { PermitLimit = limit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
});

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("Brak konfiguracji sekcji JWT.");

if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
{
    throw new InvalidOperationException("Klucz JWT nie może być pusty.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = jwtOptions.Audience,
            ValidIssuer = jwtOptions.Issuer,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var principal = context.Principal!;
                if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
                    !int.TryParse(principal.FindFirstValue("security_version"), out var version))
                { context.Fail("Sesja wygasła. Zaloguj się ponownie."); return; }
                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(item => item.Id == userId,
                    context.HttpContext.RequestAborted);
                var warehouseClaim = principal.FindFirstValue("warehouse_id");
                if (user is null || user.SecurityVersion != version ||
                    principal.FindFirstValue("role_id") != user.RoleId.ToString() ||
                    warehouseClaim != user.WarehouseId?.ToString())
                { context.Fail("Dane sesji zmieniły się. Zaloguj się ponownie."); return; }
                context.HttpContext.Items[AuthenticatedAccount.ContextKey] =
                    new AuthenticatedAccount(user.Id, user.RoleId, user.WarehouseId);
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure();
        }));

builder.Services.AddDbContext<HistoryDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("HistoryConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure();
        }));


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRepository<InventoryHistory>, HistoryRepository<InventoryHistory>>();
builder.Services.AddScoped<IRepository<ShipmentHistory>, HistoryRepository<ShipmentHistory>>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IWarehouseContext, HttpWarehouseContext>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();
builder.Services.AddScoped<IDocumentNumberGenerator, DocumentNumberGenerator>();
builder.Services.AddScoped<IShipmentReservationService, ShipmentReservationService>();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration);

// CORS runs first so error responses produced by the exception handler below still carry the CORS headers;
// otherwise the browser reports a server error as a CORS failure.
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = exception switch
    {
        ResourceNotFoundException => StatusCodes.Status404NotFound,
        ResourceConflictException => StatusCodes.Status409Conflict,
        CommandValidationException => StatusCodes.Status400BadRequest,
        ForbiddenOperationException => StatusCodes.Status403Forbidden,
        DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
        DbUpdateException => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
        context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ErrorHandler")
            .LogError(exception, "Nieobsłużony błąd w {Method} {Path}", context.Request.Method, context.Request.Path);

    var message = context.Response.StatusCode == StatusCodes.Status500InternalServerError
        ? "Wystąpił błąd serwera."
        : exception is DbUpdateException
            ? "Dane są powiązane z innymi rekordami lub naruszają ograniczenia bazy."
            : exception?.Message;

    await context.Response.WriteAsJsonAsync(new { message });
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program { }
