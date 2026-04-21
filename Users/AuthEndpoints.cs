using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "MyWebAppFastEndpoints";
    public string Audience { get; init; } = "MyWebAppFastEndpointsClient";
    public string SigningKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}

public sealed class LoginRequest
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}

public sealed class LoginResponse
{
    public required string Token { get; set; }
    public required DateTime ExpiresAtUtc { get; set; }
    public required string Role { get; set; }
}

public sealed class LoginEndpoint(IUserStore store, IPasswordHasher passwordHasher, IOptions<JwtOptions> jwtOptions) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = store.GetByLogin(req.Login);
        if (user is null || !passwordHasher.Verify(req.Password, user.PasswordHash))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var opts = jwtOptions.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(opts.ExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: opts.Issuer,
            audience: opts.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        await Send.OkAsync(new LoginResponse
        {
            Token = tokenValue,
            ExpiresAtUtc = expiresAt,
            Role = user.Role.ToString()
        }, ct);
    }
}

public sealed class ForgotPasswordRequest
{
    public required string Login { get; set; }
}

public sealed class ForgotPasswordEndpoint(IUserStore store) : Endpoint<ForgotPasswordRequest>
{
    public override void Configure()
    {
        Post("/api/auth/forgot-password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ForgotPasswordRequest req, CancellationToken ct)
    {
        // Always respond 200 regardless of whether login exists (prevents login enumeration).
        // In a real app this would trigger a password-reset email.
        _ = store.GetByLogin(req.Login);
        await Send.OkAsync(ct);
    }
}
