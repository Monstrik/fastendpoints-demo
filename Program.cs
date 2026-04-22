using System.Text;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var bld = WebApplication.CreateBuilder();

var jwtOptions = bld.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
    throw new InvalidOperationException("Jwt:SigningKey is required.");

bld.Services.AddFastEndpoints();
bld.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

bld.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(bld.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));
bld.Services.AddScoped<IUserStore, EfUserStore>();

bld.Services
    .AddOptions<JwtOptions>()
    .Bind(bld.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey), "Jwt:SigningKey is required.")
    .ValidateOnStart();

bld.Services
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

bld.Services.AddAuthorization();

var app = bld.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

SeedAdminUser(app.Services);

app.Run();

static void SeedAdminUser(IServiceProvider services)
{
    using var scope = services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var store = scope.ServiceProvider.GetRequiredService<IUserStore>();

    if (store.GetByLogin("admin") is not null)
        return;

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    _ = store.Create("admin", passwordHasher.Hash("Admin123!"), "System", "Admin", UserRole.Admin);
}


// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program;
