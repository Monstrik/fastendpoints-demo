using System.Text;
using FastEndpoints;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddFastEndpoints();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var backendRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", ".."));
        var defaultConnectionString = configuration.GetConnectionString("Default") ?? "Data Source=db/app.db";
        var connectionStringBuilder = new SqliteConnectionStringBuilder(defaultConnectionString);

        if (string.IsNullOrWhiteSpace(connectionStringBuilder.DataSource))
            connectionStringBuilder.DataSource = Path.Combine(backendRoot, "db", "app.db");
        else if (!Path.IsPathRooted(connectionStringBuilder.DataSource))
            connectionStringBuilder.DataSource = Path.GetFullPath(Path.Combine(backendRoot, connectionStringBuilder.DataSource));

        var databaseDirectory = Path.GetDirectoryName(connectionStringBuilder.DataSource);
        if (!string.IsNullOrWhiteSpace(databaseDirectory))
            Directory.CreateDirectory(databaseDirectory);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionStringBuilder.ConnectionString));

        services.AddScoped<IUserStore, EfUserStore>();
        services.AddScoped<IPostStore, EfPostStore>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
            throw new InvalidOperationException("JWT signing key is required. Set configuration key 'Jwt:SigningKey' via environment variable 'Jwt__SigningKey'.");

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "JWT signing key is required. Set environment variable 'Jwt__SigningKey'.")
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        return services;
    }
}

