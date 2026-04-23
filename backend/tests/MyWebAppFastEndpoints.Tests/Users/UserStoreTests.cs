public class UserStoreTests
{
    [Fact]
    public void Create_ThenGetById_ReturnsCreatedUser()
    {
        var store = new InMemoryUserStore();

        var created = store.Create("aya", "hash", "Aya", "Kovi", UserRole.User);
        var loaded = store.GetById(created!.Id);

        Assert.NotNull(loaded);
        Assert.Equal(created, loaded);
    }

    [Fact]
    public void Create_WhenLoginExists_ReturnsNull()
    {
        var store = new InMemoryUserStore();

        _ = store.Create("aya", "hash1", "Aya", "Kovi", UserRole.User);
        var duplicate = store.Create("aya", "hash2", "Other", "User", UserRole.Admin);

        Assert.Null(duplicate);
    }

    [Fact]
    public void GetAll_ReturnsUsersSortedByLogin()
    {
        var store = new InMemoryUserStore();

        _ = store.Create("zed", "hash", "Zed", "B", UserRole.User);
        var a2 = store.Create("aya-z", "hash", "Aya", "Z", UserRole.User);
        var a1 = store.Create("aya-a", "hash", "Aya", "A", UserRole.User);

        var users = store.GetAll();

        Assert.Collection(users,
            user => Assert.Equal(a1!.Id, user.Id),
            user => Assert.Equal(a2!.Id, user.Id),
            user => Assert.Equal("zed", user.Login));
    }

    [Fact]
    public void Update_WhenUserExists_ChangesStoredValues()
    {
        var store = new InMemoryUserStore();
        var created = store.Create("aya", "hash", "Aya", "Kovi", UserRole.User);

        var updated = store.Update(created!.Id, "aya-updated", "new-hash", "Aya", "Updated", UserRole.Admin, "🎯 Focused");

        Assert.NotNull(updated);
        Assert.Equal("aya-updated", updated!.Login);
        Assert.Equal("new-hash", updated.PasswordHash);
        Assert.Equal("Updated", updated.LastName);
        Assert.Equal(UserRole.Admin, updated.Role);
        Assert.Equal("🎯 Focused", updated.Status);

        var loaded = store.GetById(created.Id);
        Assert.Equal(updated, loaded);
    }

    [Fact]
    public void Update_WhenLoginTaken_ReturnsNull()
    {
        var store = new InMemoryUserStore();
        var first = store.Create("first", "hash", "F", "One", UserRole.User);
        var second = store.Create("second", "hash", "S", "Two", UserRole.User);

        var result = store.Update(second!.Id, "first", null, "S", "Two", UserRole.User);

        Assert.Null(result);
        Assert.Equal("first", store.GetById(first!.Id)!.Login);
        Assert.Equal("second", store.GetById(second.Id)!.Login);
    }

    [Fact]
    public void Delete_ReturnsTrueThenFalse_ForSameId()
    {
        var store = new InMemoryUserStore();
        var created = store.Create("aya", "hash", "Aya", "Kovi", UserRole.User);

        var firstDelete = store.Delete(created!.Id);
        var secondDelete = store.Delete(created.Id);

        Assert.True(firstDelete);
        Assert.False(secondDelete);
    }
}
