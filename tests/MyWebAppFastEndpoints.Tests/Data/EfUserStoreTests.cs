using Microsoft.EntityFrameworkCore;

public class EfUserStoreTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Create_WhenValidData_ReturnsCreatedUser()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var user = store.Create("testuser", "hash123", "Test", "User", UserRole.User);

        Assert.NotNull(user);
        Assert.Equal("testuser", user!.Login);
        Assert.Equal("Test", user.FirstName);
        Assert.Equal("User", user.LastName);
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal(UserStatuses.Default, user.Status);
    }

    [Fact]
    public void Create_WhenLoginAlreadyExists_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        _ = store.Create("existing", "hash1", "First", "User", UserRole.User);
        var duplicate = store.Create("existing", "hash2", "Second", "User", UserRole.User);

        Assert.Null(duplicate);
    }

    [Fact]
    public void Create_WithCustomStatus_PersistsStatus()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var user = store.Create("statususer", "hash", "Status", "User", UserRole.User, "🎯 Focused");

        Assert.NotNull(user);
        Assert.Equal("🎯 Focused", user!.Status);
    }

    [Fact]
    public void GetById_WhenUserExists_ReturnsUser()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var created = store.Create("findme", "hash", "Find", "Me", UserRole.User);
        var found = store.GetById(created!.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found!.Id);
        Assert.Equal("findme", found.Login);
    }

    [Fact]
    public void GetById_WhenUserDoesNotExist_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var found = store.GetById(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public void GetByLogin_WhenUserExists_ReturnsUser()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var created = store.Create("searchme", "hash", "Search", "Me", UserRole.User);
        var found = store.GetByLogin("searchme");

        Assert.NotNull(found);
        Assert.Equal(created.Id, found!.Id);
    }

    [Fact]
    public void GetByLogin_TrimsInputButIsCaseSensitive()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        _ = store.Create("CaseSensitive", "hash", "Case", "Sensitive", UserRole.User);
        var foundExact = store.GetByLogin("CaseSensitive");
        var foundWrong = store.GetByLogin("casesensitive");

        Assert.NotNull(foundExact);
        Assert.Null(foundWrong);
    }

    [Fact]
    public void GetByLogin_WhenUserDoesNotExist_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var found = store.GetByLogin("nonexistent");

        Assert.Null(found);
    }

    [Fact]
    public void GetAll_ReturnsSortedByLogin()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        store.Create("zebra", "hash", "Z", "User", UserRole.User);
        store.Create("apple", "hash", "A", "User", UserRole.User);
        store.Create("banana", "hash", "B", "User", UserRole.User);

        var all = store.GetAll();

        Assert.Equal(3, all.Count);
        Assert.Equal("apple", all[0]!.Login);
        Assert.Equal("banana", all[1]!.Login);
        Assert.Equal("zebra", all[2]!.Login);
    }

    [Fact]
    public void Update_WhenUserExists_UpdatesProperties()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var user = store.Create("updateme", "hash1", "Update", "Me", UserRole.User, UserStatuses.Default);
        var updated = store.Update(user!.Id, "updated", "newhash", "Updated", "User", UserRole.Admin, "🔴 Busy");

        Assert.NotNull(updated);
        Assert.Equal("updated", updated!.Login);
        Assert.Equal("newhash", updated.PasswordHash);
        Assert.Equal("Updated", updated.FirstName);
        Assert.Equal("User", updated.LastName);
        Assert.Equal(UserRole.Admin, updated.Role);
        Assert.Equal("🔴 Busy", updated.Status);
    }

    [Fact]
    public void Update_WithNullPassword_KeepsExistingPassword()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var user = store.Create("keeppass", "originalHash", "Keep", "Pass", UserRole.User);
        var updated = store.Update(user!.Id, "keeppass", null, "Keep", "Pass", UserRole.User);

        Assert.NotNull(updated);
        Assert.Equal("originalHash", updated!.PasswordHash);
    }

    [Fact]
    public void Update_WhenLoginTaken_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var user1 = store.Create("first", "hash", "First", "User", UserRole.User);
        var user2 = store.Create("second", "hash", "Second", "User", UserRole.User);

        var result = store.Update(user2!.Id, "first", null, "Second", "User", UserRole.User);

        Assert.Null(result);
    }

    [Fact]
    public void Update_WhenUserDoesNotExist_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var result = store.Update(Guid.NewGuid(), "newlogin", "hash", "New", "Login", UserRole.User);

        Assert.Null(result);
    }

    [Fact]
    public void Update_WithSameLoginDifferentCase_RequiresExactMatch()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var user = store.Create("MixedCase", "hash", "Mixed", "Case", UserRole.User);
        var updated = store.Update(user!.Id, "MixedCase", "hash2", "Mixed", "Case", UserRole.User);

        Assert.NotNull(updated);
        Assert.Equal("MixedCase", updated!.Login);
    }

    [Fact]
    public void Delete_WhenUserExists_RemovesUser()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var user = store.Create("deleteme", "hash", "Delete", "Me", UserRole.User);
        var deleted = store.Delete(user!.Id);
        var found = store.GetById(user.Id);

        Assert.True(deleted);
        Assert.Null(found);
    }

    [Fact]
    public void Delete_WhenUserDoesNotExist_ReturnsFalse()
    {
        var db = CreateDbContext();
        var store = new EfUserStore(db);

        var deleted = store.Delete(Guid.NewGuid());

        Assert.False(deleted);
    }
}

