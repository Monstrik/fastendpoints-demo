namespace MyWebAppFastEndpoints.Infrastructure;

using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Events;

/// <summary>
/// Extension methods for configuring Serilog logging.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds and configures Serilog for the application.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    /// <summary>
    /// Ensures Serilog is flushed on application shutdown.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void UseSerilogShutdown(this WebApplication app)
    {
        app.Lifetime.ApplicationStopped.Register(() => Log.CloseAndFlush());
    }
}

