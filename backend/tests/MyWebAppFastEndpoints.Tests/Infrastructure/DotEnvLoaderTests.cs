using MyWebAppFastEndpoints.Infrastructure;

public class DotEnvLoaderTests : IDisposable
{
    // Unique prefix per test instance so parallel tests don't interfere.
    private readonly string _prefix = $"DOTENV_TEST_{Guid.NewGuid():N}_";
    private readonly List<string> _tempFiles = new();

    // ── TryParseLine ──────────────────────────────────────────────────────────

    [Fact]
    public void TryParseLine_EmptyLine_ReturnsFalse()
    {
        Assert.False(DotEnvLoader.TryParseLine("", out _, out _));
    }

    [Fact]
    public void TryParseLine_WhitespaceLine_ReturnsFalse()
    {
        Assert.False(DotEnvLoader.TryParseLine("   ", out _, out _));
    }

    [Fact]
    public void TryParseLine_CommentLine_ReturnsFalse()
    {
        Assert.False(DotEnvLoader.TryParseLine("# this is a comment", out _, out _));
    }

    [Fact]
    public void TryParseLine_InlineCommentLine_ReturnsFalse()
    {
        // A line that starts with # after trimming
        Assert.False(DotEnvLoader.TryParseLine("  # indented comment", out _, out _));
    }

    [Fact]
    public void TryParseLine_NoEqualsSign_ReturnsFalse()
    {
        Assert.False(DotEnvLoader.TryParseLine("JUST_A_KEY", out _, out _));
    }

    [Fact]
    public void TryParseLine_EqualsAtStart_ReturnsFalse()
    {
        Assert.False(DotEnvLoader.TryParseLine("=value", out _, out _));
    }

    [Fact]
    public void TryParseLine_SimpleKeyValue_Parses()
    {
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY=hello", out var key, out var value));
        Assert.Equal("MY_KEY", key);
        Assert.Equal("hello", value);
    }

    [Fact]
    public void TryParseLine_KeyValueWithSpacesAroundEquals_Parses()
    {
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY = hello", out var key, out var value));
        Assert.Equal("MY_KEY", key);
        Assert.Equal("hello", value);
    }

    [Fact]
    public void TryParseLine_EmptyValue_ParsesEmptyString()
    {
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY=", out var key, out var value));
        Assert.Equal("MY_KEY", key);
        Assert.Equal("", value);
    }

    [Fact]
    public void TryParseLine_DoubleQuotedValue_StripsQuotes()
    {
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY=\"hello world\"", out _, out var value));
        Assert.Equal("hello world", value);
    }

    [Fact]
    public void TryParseLine_SingleQuotedValue_StripsQuotes()
    {
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY='hello world'", out _, out var value));
        Assert.Equal("hello world", value);
    }

    [Fact]
    public void TryParseLine_InlineComment_StripsComment()
    {
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY=hello # a comment", out _, out var value));
        Assert.Equal("hello", value);
    }

    [Fact]
    public void TryParseLine_QuotedValueWithHash_PreservesHash()
    {
        // Hash inside quotes is not a comment
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY=\"hello#world\"", out _, out var value));
        Assert.Equal("hello#world", value);
    }

    [Fact]
    public void TryParseLine_ExportPrefix_StripsExportAndParses()
    {
        Assert.True(DotEnvLoader.TryParseLine("export MY_KEY=my_value", out var key, out var value));
        Assert.Equal("MY_KEY", key);
        Assert.Equal("my_value", value);
    }

    [Fact]
    public void TryParseLine_ExportPrefixWithExtraSpaces_StripsAndParses()
    {
        Assert.True(DotEnvLoader.TryParseLine("export  MY_KEY=my_value", out var key, out var value));
        Assert.Equal("MY_KEY", key);
        Assert.Equal("my_value", value);
    }

    [Fact]
    public void TryParseLine_ValueWithEqualsSign_PreservesRemainder()
    {
        // Only the first = splits key from value
        Assert.True(DotEnvLoader.TryParseLine("MY_KEY=a=b=c", out var key, out var value));
        Assert.Equal("MY_KEY", key);
        Assert.Equal("a=b=c", value);
    }

    // ── Load (single file) ────────────────────────────────────────────────────

    [Fact]
    public void Load_SetsVariablesFromFile()
    {
        var key = Key("VAR");
        var file = WriteTempFile($"{key}=loaded_value\n");

        DotEnvLoader.Load(file);

        Assert.Equal("loaded_value", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_SkipsExistingProcessEnvVar()
    {
        var key = Key("EXISTING");
        Environment.SetEnvironmentVariable(key, "original");

        var file = WriteTempFile($"{key}=overwritten\n");
        DotEnvLoader.Load(file);

        Assert.Equal("original", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_WithOverwriteFileSources_DoesNotOverwriteProcessEnvVar()
    {
        var key = Key("PROC_VAR");
        // Set directly as process env (no marker)
        Environment.SetEnvironmentVariable(key, "from_process");

        var file = WriteTempFile($"{key}=from_file\n");
        DotEnvLoader.Load(file, overwriteFileSources: true);

        Assert.Equal("from_process", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_WithOverwriteFileSources_OverwritesPreviousFileLoadedVar()
    {
        var key = Key("FILE_VAR");

        // Simulate a first file load (sets the marker)
        var firstFile = WriteTempFile($"{key}=first_value\n");
        DotEnvLoader.Load(firstFile, overwriteFileSources: false);
        Assert.Equal("first_value", Environment.GetEnvironmentVariable(key));

        // Now load a second file with overwrite=true — should replace
        var secondFile = WriteTempFile($"{key}=second_value\n");
        DotEnvLoader.Load(secondFile, overwriteFileSources: true);

        Assert.Equal("second_value", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_SetsMarkerForFileLoadedKeys()
    {
        var key = Key("MARKED");
        var file = WriteTempFile($"{key}=some_value\n");

        DotEnvLoader.Load(file);

        Assert.Equal("1", Environment.GetEnvironmentVariable($"_DotEnv_{key}"));
    }

    [Fact]
    public void Load_IgnoresCommentAndBlankLines()
    {
        var key = Key("REAL");
        var file = WriteTempFile($"""
            # comment
            
            {key}=real_value
            # another comment
            """);

        DotEnvLoader.Load(file);

        Assert.Equal("real_value", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void Load_ParsesMultipleVariables()
    {
        var key1 = Key("MULTI1");
        var key2 = Key("MULTI2");
        var file = WriteTempFile($"{key1}=val1\n{key2}=val2\n");

        DotEnvLoader.Load(file);

        Assert.Equal("val1", Environment.GetEnvironmentVariable(key1));
        Assert.Equal("val2", Environment.GetEnvironmentVariable(key2));
    }

    // ── LoadFromDefaultLocations (.env + .env.local) ─────────────────────────

    [Fact]
    public void LoadFromDefaultLocations_LoadsEnvFile()
    {
        var key = Key("FROM_ENV");
        var dir = CreateTempDir();
        WriteTempFileIn(dir, ".env", $"{key}=env_value\n");

        RunInDirectory(dir, DotEnvLoader.LoadFromDefaultLocations);

        Assert.Equal("env_value", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void LoadFromDefaultLocations_EnvLocalOverridesEnv()
    {
        var key = Key("OVERRIDE");
        var dir = CreateTempDir();
        WriteTempFileIn(dir, ".env", $"{key}=from_env\n");
        WriteTempFileIn(dir, ".env.local", $"{key}=from_local\n");

        RunInDirectory(dir, DotEnvLoader.LoadFromDefaultLocations);

        Assert.Equal("from_local", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void LoadFromDefaultLocations_ProcessEnvBeatsEnvLocal()
    {
        var key = Key("PROC_WINS");
        Environment.SetEnvironmentVariable(key, "from_process");

        var dir = CreateTempDir();
        WriteTempFileIn(dir, ".env", $"{key}=from_env\n");
        WriteTempFileIn(dir, ".env.local", $"{key}=from_local\n");

        RunInDirectory(dir, DotEnvLoader.LoadFromDefaultLocations);

        Assert.Equal("from_process", Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void LoadFromDefaultLocations_NoEnvFile_DoesNothing()
    {
        var key = Key("MISSING_ENV");
        var emptyDir = CreateTempDir();

        // Should not throw
        RunInDirectory(emptyDir, DotEnvLoader.LoadFromDefaultLocations);

        Assert.Null(Environment.GetEnvironmentVariable(key));
    }

    [Fact]
    public void LoadFromDefaultLocations_NoEnvLocal_StillLoadsEnv()
    {
        var key = Key("NO_LOCAL");
        var dir = CreateTempDir();
        WriteTempFileIn(dir, ".env", $"{key}=env_only\n");
        // no .env.local

        RunInDirectory(dir, DotEnvLoader.LoadFromDefaultLocations);

        Assert.Equal("env_only", Environment.GetEnvironmentVariable(key));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string Key(string name) => $"{_prefix}{name}";

    private string WriteTempFile(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void WriteTempFileIn(string dir, string fileName, string content)
    {
        File.WriteAllText(Path.Combine(dir, fileName), content);
    }

    /// <summary>
    /// Runs action with current directory temporarily set to <paramref name="dir"/>.
    /// </summary>
    private static void RunInDirectory(string dir, Action action)
    {
        var original = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(dir);
            action();
        }
        finally
        {
            Directory.SetCurrentDirectory(original);
        }
    }

    public void Dispose()
    {
        // Clean up temp files
        foreach (var file in _tempFiles)
            if (File.Exists(file)) File.Delete(file);

        // Unset all test-scoped env vars
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            var k = entry.Key?.ToString() ?? "";
            if (k.StartsWith(_prefix, StringComparison.Ordinal) ||
                k.StartsWith($"_DotEnv_{_prefix}", StringComparison.Ordinal))
            {
                Environment.SetEnvironmentVariable(k, null);
            }
        }
    }
}

