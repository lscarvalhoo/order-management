using System.Diagnostics.CodeAnalysis;
using OrderManagement.API.Extensions;
using OrderManagement.Application.Extensions;
using OrderManagement.Infrastructure.Extensions;
using OrderManagement.Infrastructure.Persistence;
using Serilog;

var isTesting = args.Contains("--environment=Testing") || 
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Testing", StringComparison.OrdinalIgnoreCase) == true;

if (!isTesting)
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
        .CreateBootstrapLogger();

    Log.Information("Starting OrderManagement API");
}

var builder = WebApplication.CreateBuilder(args);

if (!isTesting)
{
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));
}

if (!isTesting)
{
    builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.ApplyMigrations();
    app.UseSerilogRequestLogging();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

if (!isTesting)
{
    Log.Information("OrderManagement API started successfully");
}
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
