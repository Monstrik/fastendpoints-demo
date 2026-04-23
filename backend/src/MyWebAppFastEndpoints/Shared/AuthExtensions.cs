namespace MyWebAppFastEndpoints.Shared;

using System.Security.Claims;

/// <summary>
/// Extension methods for extracting authenticated user information from claims.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Extracts the user ID from the current authenticated user's claims.
    /// </summary>
    /// <param name="claimsPrincipal">The current user's ClaimsPrincipal.</param>
    /// <returns>The user's ID, or null if not authenticated or ID is invalid.</returns>
    public static Guid? GetUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal is null)
            return null;

        var idRaw = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idRaw, out var id) ? id : null;
    }
}
