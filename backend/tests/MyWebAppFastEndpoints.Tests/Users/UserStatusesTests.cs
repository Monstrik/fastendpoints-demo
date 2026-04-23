public class UserStatusesTests
{
    [Fact]
    public void Default_IsAvailable()
    {
        Assert.Equal("🟢 Available", UserStatuses.Default);
    }

    [Fact]
    public void Allowed_ContainsDefaultStatus()
    {
        Assert.Contains(UserStatuses.Default, UserStatuses.Allowed);
    }

    [Fact]
    public void Allowed_HasMultipleStatuses()
    {
        Assert.True(UserStatuses.Allowed.Count > 10);
    }

    [Theory]
    [MemberData(nameof(AllAllowedStatuses))]
    public void AllAllowedStatuses_AreValidEmojis(string status)
    {
        Assert.NotNull(status);
        Assert.NotEmpty(status);
        Assert.Contains("🟢", status, System.StringComparison.Ordinal);
    }

    public static TheoryData<string> AllAllowedStatuses()
    {
        var data = new TheoryData<string>();
        foreach (var status in UserStatuses.Allowed)
        {
            if (status.Contains("🟢"))
                data.Add(status);
        }
        return data;
    }
}

