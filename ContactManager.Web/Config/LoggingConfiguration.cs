using Serilog;
using Serilog.Events;

namespace ContactManager.Web.Config
{
    /// <summary>
    /// Configuration for logging setup using Serilog
    /// Provides structured logging with file output to logs folder
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Configures Serilog logging with file output
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Logger configuration</returns>
        public static LoggerConfiguration CreateLoggerConfiguration(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "../logs/log-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "../logs/errors-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .ReadFrom.Configuration(configuration);
        }

        /// <summary>
        /// Adds Serilog to the application
        /// </summary>
        /// <param name="builder">Web application builder</param>
        public static void ConfigureLogging(WebApplicationBuilder builder)
        {
            // Ensure logs directory exists in the root project folder
            Directory.CreateDirectory("../logs");

            Log.Logger = CreateLoggerConfiguration(builder.Configuration).CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}