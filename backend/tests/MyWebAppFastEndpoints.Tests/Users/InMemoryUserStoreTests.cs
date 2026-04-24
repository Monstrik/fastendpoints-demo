public class InMemoryUserStoreTests
{
    [Fact]
    public void Create_ReturnsDuplicateLoginAsNull()
    {
        var store = new InMemoryUserStore();

        var first = store.Create("duplicate", "hash1", "First", "User", UserRole.User);
        var second = store.Create("duplicate", "hash2", "Second", "User", UserRole.User);

        Assert.NotNull(first);
        Assert.Null(second);
    }

    [Fact]
    public void Create_IsCaseInsensitiveForLogin()
    {
        var store = new InMemoryUserStore();

        var lower = store.Create("testuser", "hash1", "Test", "User", UserRole.User);
        var upper = store.Create("TESTUSER", "hash2", "Test", "User", UserRole.User);

        Assert.NotNull(lower);
        Assert.Null(upper);
    }

    [Fact]
    public void Create_TrimsLogin()
    {
        var store = new InMemoryUserStore();

        var user = store.Create("  padded  ", "hash", "Padded", "User", UserRole.User);

        Assert.NotNull(user);
        Assert.Equal("padded", user!.Login);
    }

    [Fact]
    public void Update_IsCaseInsensitiveForLogin()
    {
        var store = new InMemoryUserStore();

        var user1 = store.Create("user1", "hash", "User", "One", UserRole.User);
        var user2 = store.Create("user2", "hash", "User", "Two", UserRole.User);

        var updated = store.Update(user2!.Id, "USER1", "hash", "User", "Two", UserRole.User);

        Assert.Null(updated);
    }

    [Fact]
    public void Update_AllowsSameLoginWithDifferentCase()
    {
        var store = new InMemoryUserStore();

        var user = store.Create("MixedCase", "hash", "Mixed", "Case", UserRole.User);
        var updated = store.Update(user!.Id, "mixedcase", "hash", "Mixed", "Case", UserRole.User);

        Assert.NotNull(updated);
        Assert.Equal("mixedcase", updated!.Login);
    }

    [Fact]
    public void GetByLogin_IsCaseInsensitive()
    {
        var store = new InMemoryUserStore();

        store.Create("CaseTest", "hash", "Case", "Test", UserRole.User);
        var found1 = store.GetByLogin("CASETEST");
        var found2 = store.GetByLogin("casetest");

        Assert.NotNull(found1);
        Assert.NotNull(found2);
        Assert.Equal(found1!.Id, found2!.Id);
    }

    [Fact]
    public async Task ConcurrentOperations_AreThreadSafe()
    {
        var store = new InMemoryUserStore();
        var results = new System.Collections.Concurrent.ConcurrentBag<Guid?>();

        var tasks = new System.Collections.Generic.List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            var login = $"concurrent-{index}";
            tasks.Add(Task.Run(() =>
            {
                var user = store.Create(login, "hash", "Concurrent", "User", UserRole.User);
                if (user != null)
                {
                    results.Add(user.Id);
                }
            }));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(10, results.Count);
        Assert.Equal(10, new System.Collections.Generic.HashSet<Guid>(results.Where(id => id.HasValue).Select(id => id!.Value)).Count); // All unique
    }
}

