using ContactManager.Web.Config;
using ContactManager.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure logging first
LoggingConfiguration.ConfigureLogging(builder);

// Configure all services using centralized configuration
ApplicationConfiguration.ConfigureServices(builder.Services, builder.Configuration);

// Add WebSocket support
builder.AddWebSocketSupport();

var app = builder.Build();

// Initialize database
try
{
    Log.Information("Starting database initialization");
    await DatabaseConfiguration.InitializeDatabaseAsync(app);
    Log.Information("Database initialization completed successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database initialization failed");
    throw;
}

// Configure HTTP pipeline using centralized configuration
ApplicationConfiguration.ConfigurePipeline(app);

// Add WebSocket middleware
app.UseWebSocketMiddleware();

app.Run();