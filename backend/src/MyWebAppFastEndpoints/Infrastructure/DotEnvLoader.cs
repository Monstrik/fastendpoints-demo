namespace MyWebAppFastEndpoints.Infrastructure;

public static class DotEnvLoader
{
    public static void LoadFromDefaultLocations()
    {
        foreach (var filePath in GetDefaultCandidatePaths())
        {
            if (!File.Exists(filePath))
                continue;

            Load(filePath);
            break;
        }
    }

    public static void Load(string filePath)
    {
        foreach (var rawLine in File.ReadLines(filePath))
        {
            if (!TryParseLine(rawLine, out var key, out var value))
                continue;

            // Keep process/CI environment variables as the highest-priority source.
            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
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

    private static IEnumerable<string> GetDefaultCandidatePaths()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            yield return Path.Combine(current.FullName, ".env");
            current = current.Parent;
        }
    }
}

