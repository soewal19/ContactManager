using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ContactManager.Infrastructure.Data;
using System.Diagnostics;

namespace ContactManager.Web.Config
{
    /// <summary>
    /// Configuration class for database setup and initialization
    /// Follows best practices for Entity Framework Core configuration
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Configures database context based on connection string
        /// Supports both SQLite and SQL Server with automatic detection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ApplicationDbContext>();
                
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("DefaultConnection connection string is not configured. Please check your appsettings.json file.");
                }

                // Auto-detect database type based on connection string
                if (IsSqliteConnection(connectionString))
                {
                    logger.LogInformation("Configuring SQLite database");
                    ConfigureSqlite(options, connectionString);
                }
                else if (IsSqlServerConnection(connectionString))
                {
                    logger.LogInformation("Configuring SQL Server database");
                    ConfigureSqlServer(options, connectionString);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported database connection string format. Connection string: {connectionString}");
                }
            });
        }

        /// <summary>
        /// Initializes database on application startup
        /// Handles migrations for SQL Server and table creation for SQLite
        /// </summary>
        /// <param name="app">Web application</param>
        public static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ApplicationDbContext>();
            
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var stopwatch = Stopwatch.StartNew();
                
                logger.LogInformation("Starting database initialization");

                if (context.Database.IsSqlite())
                {
                    await InitializeSqliteDatabaseAsync(context, logger);
                }
                else if (context.Database.IsSqlServer())
                {
                    await InitializeSqlServerDatabaseAsync(context, logger);
                }
                
                stopwatch.Stop();
                logger.LogInformation($"Database initialization completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization");
                throw;
            }
        }

        /// <summary>
        /// Configures SQLite database options
        /// </summary>
        private static void ConfigureSqlite(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(30); // 30 seconds timeout
                sqliteOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });
        }

        /// <summary>
        /// Configures SQL Server database options
        /// </summary>
        private static void ConfigureSqlServer(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(30); // 30 seconds timeout
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });
        }

        /// <summary>
        /// Initializes SQLite database using EnsureCreated approach
        /// </summary>
        private static async Task InitializeSqliteDatabaseAsync(ApplicationDbContext context, ILogger logger)
        {
            logger.LogInformation("Initializing SQLite database");
            
            try
            {
                // For development, optionally delete existing database
                if (context.Database.GetDbConnection().DataSource is string dbPath && File.Exists(dbPath))
                {
                    logger.LogInformation("Found existing SQLite database, will recreate for clean state");
                    
                    try
                    {
                        context.Database.CloseConnection();
                        File.Delete(dbPath);
                        logger.LogInformation("Existing SQLite database deleted successfully");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Could not delete existing database file, using EnsureDeleted instead");
                        await context.Database.EnsureDeletedAsync();
                    }
                }
                
                // Create database and tables
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("SQLite database and tables created successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize SQLite database");
                throw;
            }
        }

        /// <summary>
        /// Initializes SQL Server database using migrations
        /// </summary>
        private static async Task InitializeSqlServerDatabaseAsync(ApplicationDbContext context, ILogger logger)
        {
            logger.LogInformation("Initializing SQL Server database");
            
            try
            {
                // Apply pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingCount = pendingMigrations.Count();
                
                if (pendingCount > 0)
                {
                    logger.LogInformation($"Applying {pendingCount} pending migrations: {string.Join(", ", pendingMigrations)}");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("SQL Server database migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("No pending migrations found, database is up to date");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize SQL Server database");
                throw;
            }
        }

        /// <summary>
        /// Determines if connection string is for SQLite
        /// </summary>
        private static bool IsSqliteConnection(string connectionString)
        {
            return connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("DataSource=", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if connection string is for SQL Server
        /// </summary>
        private static bool IsSqlServerConnection(string connectionString)
        {
            return connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && 
                   connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase);
        }
    }
}