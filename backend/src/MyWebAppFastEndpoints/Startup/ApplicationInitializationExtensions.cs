using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ApplicationInitializationExtensions
{
    private static readonly object SeedLock = new();

    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        if (IsSwaggerEnabled(app.Configuration))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints();
        return app;
    }

    private static bool IsSwaggerEnabled(IConfiguration configuration)
    {
        var raw = configuration["ENABLE_SWAGGER"];
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (bool.TryParse(raw, out var enabled))
            return enabled;

        return string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "on", StringComparison.OrdinalIgnoreCase);
    }

    public static void SeedAdminUser(this WebApplication app)
    {
        lock (SeedLock)
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var store = scope.ServiceProvider.GetRequiredService<IUserStore>();
            if (store.GetByLogin("admin") is not null)
                return;

            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            _ = store.Create("admin", passwordHasher.Hash("Admin123!"), "System", "Admin", UserRole.Admin);
        }
    }
}
