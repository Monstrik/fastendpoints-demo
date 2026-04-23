public class PasswordHasherEdgeCasesTests
{
    [Fact]
    public void Hash_ProducesDifferentResultsEachTime()
    {
        var hasher = new PasswordHasher();
        var password = "TestPassword123!";

        var hash1 = hasher.Hash(password);
        var hash2 = hasher.Hash(password);

        Assert.NotEqual(hash1, hash2);
        Assert.True(hasher.Verify(password, hash1));
        Assert.True(hasher.Verify(password, hash2));
    }

    [Fact]
    public void Verify_ReturnsFalseForEmptyHash()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.Verify("password", ""));
        Assert.False(hasher.Verify("password", "   "));
    }

    [Fact]
    public void Verify_ReturnsFalseForMalformedHash()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.Verify("password", "not.a.valid.hash"));
        Assert.False(hasher.Verify("password", "only.two"));
        Assert.False(hasher.Verify("password", "notanumber.base64.base64"));
    }

    [Fact]
    public void Verify_ReturnsFalseForCorruptedHash()
    {
        var hasher = new PasswordHasher();
        var originalHash = hasher.Hash("OriginalPassword");

        // Change one character in the hash
        var corruptedHash = originalHash.Substring(0, originalHash.Length - 1) + "X";

        Assert.False(hasher.Verify("OriginalPassword", corruptedHash));
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Password123!");

        Assert.True(hasher.Verify("Password123!", hash));
        Assert.False(hasher.Verify("password123!", hash));
    }

    [Fact]
    public void Hash_HandlesSpecialCharacters()
    {
        var hasher = new PasswordHasher();
        var specialPasswords = new[] 
        { 
            "!@#$%^&*()",
            "with spaces",
            "ñáéíóú",
            "🔐🔒🔓",
            "very-long-password-with-many-special-chars-!@#$%^&*()",
        };

        foreach (var password in specialPasswords)
        {
            var hash = hasher.Hash(password);
            Assert.True(hasher.Verify(password, hash));
            Assert.False(hasher.Verify(password + "extra", hash));
        }
    }

    [Fact]
    public void Hash_Format_IsConsistent()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("TestPassword");

        var parts = hash.Split('.');
        Assert.Equal(3, parts.Length);
        Assert.True(int.TryParse(parts[0], out _)); // Should be iterations
    }
}

