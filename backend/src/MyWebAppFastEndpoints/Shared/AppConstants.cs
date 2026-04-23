namespace MyWebAppFastEndpoints.Shared;

/// <summary>
/// Application-wide constants for endpoints, validation, and domain constraints.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Maximum length for post content (Twitter-like limit).
    /// </summary>
    public const int PostMaxContentLength = 280;

    /// <summary>
    /// Reaction types as string constants for validation.
    /// </summary>
    public static class Reactions
    {
        public const string Like = "like";
        public const string Dislike = "dislike";
    }

    /// <summary>
    /// Default JWT configuration values.
    /// </summary>
    public static class Jwt
    {
        public const string DefaultIssuer = "MyWebAppFastEndpoints";
        public const string DefaultAudience = "MyWebAppFastEndpointsClient";
        public const int DefaultExpirationMinutes = 60;
    }
}

