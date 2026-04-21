public class UserStoreTests
{
    [Fact]
    public void Create_ThenGetById_ReturnsCreatedUser()
    {
        var store = new InMemoryUserStore();

        var created = store.Create("Aya", "Kovi", 21);
        var loaded = store.GetById(created.Id);

        Assert.NotNull(loaded);
        Assert.Equal(created, loaded);
    }

    [Fact]
    public void GetAll_ReturnsUsersSortedByFirstNameThenLastName()
    {
        var store = new InMemoryUserStore();

        _ = store.Create("Zed", "B", 20);
        var a2 = store.Create("Aya", "Z", 20);
        var a1 = store.Create("Aya", "A", 20);

        var users = store.GetAll();

        Assert.Collection(users,
            user => Assert.Equal(a1.Id, user.Id),
            user => Assert.Equal(a2.Id, user.Id),
            user => Assert.Equal("Zed", user.FirstName));
    }

    [Fact]
    public void Update_WhenUserExists_ChangesStoredValues()
    {
        var store = new InMemoryUserStore();
        var created = store.Create("Aya", "Kovi", 21);

        var updated = store.Update(created.Id, "Aya", "Updated", 17);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.LastName);
        Assert.Equal(17, updated.Age);

        var loaded = store.GetById(created.Id);
        Assert.Equal(updated, loaded);
    }

    [Fact]
    public void Update_WhenUserMissing_ReturnsNull()
    {
        var store = new InMemoryUserStore();

        var result = store.Update(Guid.NewGuid(), "A", "B", 1);

        Assert.Null(result);
    }

    [Fact]
    public void Delete_ReturnsTrueThenFalse_ForSameId()
    {
        var store = new InMemoryUserStore();
        var created = store.Create("Aya", "Kovi", 21);

        var firstDelete = store.Delete(created.Id);
        var secondDelete = store.Delete(created.Id);

        Assert.True(firstDelete);
        Assert.False(secondDelete);
    }
}

