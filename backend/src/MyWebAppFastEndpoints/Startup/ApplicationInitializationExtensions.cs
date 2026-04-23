using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class ApplicationInitializationExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints();
        return app;
    }

    public static void SeedAdminUser(this WebApplication app)
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
