namespace MyWebAppFastEndpoints.Infrastructure;

public static class DotEnvLoader
{
    /// <summary>
    /// Loads environment variables from the first .env found walking up from the
    /// current directory, then overlays values from a sibling .env.local (if present).
    /// Priority (highest → lowest): process env > .env.local > .env
    /// </summary>
    public static void LoadFromDefaultLocations()
    {
        string? envFile = null;

        foreach (var filePath in GetDefaultCandidatePaths(".env"))
        {
            if (!File.Exists(filePath))
                continue;

            envFile = filePath;
            break;
        }

        // Load base .env first (lowest priority among files).
        if (envFile is not null)
            Load(envFile, overwriteFileSources: false);

        // Load .env.local from same directory – overrides .env values but not process/shell env.
        if (envFile is not null)
        {
            var localFile = Path.Combine(Path.GetDirectoryName(envFile)!, ".env.local");
            if (File.Exists(localFile))
                Load(localFile, overwriteFileSources: true);
        }
    }

    /// <summary>
    /// Loads environment variables from a single file.
    /// When <paramref name="overwrite"/> is true, existing env vars set by a previous
    /// file load are replaced; process-level variables are never overwritten.
    /// </summary>
    public static void Load(string filePath, bool overwriteFileSources = false)
    {
        foreach (var rawLine in File.ReadLines(filePath))
        {
            if (!TryParseLine(rawLine, out var key, out var value))
                continue;

            var existing = Environment.GetEnvironmentVariable(key);

            // Process/shell/CI variables (set before any file load) always win.
            // The _DotEnv_ prefix marks keys that were set by a previous file load —
            // only those can be overridden by a higher-priority file like .env.local.
            var setByFileLoad = Environment.GetEnvironmentVariable($"_DotEnv_{key}") == "1";

            if (existing is not null && !(overwriteFileSources && setByFileLoad))
                continue;

            Environment.SetEnvironmentVariable(key, value);
            Environment.SetEnvironmentVariable($"_DotEnv_{key}", "1");
        }
    }

    public static bool TryParseLine(string rawLine, out string key, out string value)
    {
        key = string.Empty;
        value = string.Empty;

        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
            return false;

        if (line.StartsWith("export ", StringComparison.Ordinal))
            line = line[7..].TrimStart();

        var equalsIndex = line.IndexOf('=');
        if (equalsIndex <= 0)
            return false;

        key = line[..equalsIndex].Trim();
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var rawValue = line[(equalsIndex + 1)..].Trim();

        if (rawValue.Length >= 2 && ((rawValue[0] == '"' && rawValue[^1] == '"') || (rawValue[0] == '\'' && rawValue[^1] == '\'')))
        {
            value = rawValue[1..^1];
            return true;
        }

        var inlineCommentIndex = rawValue.IndexOf(" #", StringComparison.Ordinal);
        value = inlineCommentIndex >= 0 ? rawValue[..inlineCommentIndex].TrimEnd() : rawValue;
        return true;
    }

    private static IEnumerable<string> GetDefaultCandidatePaths(string fileName)
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            yield return Path.Combine(current.FullName, fileName);
            current = current.Parent;
        }
    }
}

