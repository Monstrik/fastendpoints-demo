public class PasswordHasherTests
{
    [Fact]
    public void HashThenVerify_ReturnsTrueForCorrectPassword()
    {
        var hasher = new PasswordHasher();

        var hash = hasher.Hash("Admin123!");

        Assert.True(hasher.Verify("Admin123!", hash));
        Assert.False(hasher.Verify("WrongPassword", hash));
    }
}

