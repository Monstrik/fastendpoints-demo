using Microsoft.EntityFrameworkCore;

public class PostReactionEdgeCasesTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private void SeedAdminUser(AppDbContext db)
    {
        db.Users.Add(new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = "admin",
            PasswordHash = "hash",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            Status = UserStatuses.Default
        });
        db.SaveChanges();
    }

    [Fact]
    public void MultipleUsersReactingToDifferentTypes_CountsCorrectly()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "Multiple reactions");
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        store.SetReaction(post.Id, user1, PostReactionType.Like);
        store.SetReaction(post.Id, user2, PostReactionType.Like);
        store.SetReaction(post.Id, user3, PostReactionType.Dislike);

        var retrieved = store.GetPublic()[0];
        
        Assert.Equal(2, retrieved.LikesCount);
        Assert.Equal(1, retrieved.DislikesCount);
    }

    [Fact]
    public void SwitchingReactionBetweenTypes_UpdatesCountsCorrectly()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var userId = Guid.NewGuid();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "Switch reaction");
        
        store.SetReaction(post.Id, userId, PostReactionType.Like);
        var first = store.GetPublic()[0];
        Assert.Equal(1, first.LikesCount);
        Assert.Equal(0, first.DislikesCount);

        store.SetReaction(post.Id, userId, PostReactionType.Dislike);
        var second = store.GetPublic()[0];
        Assert.Equal(0, second.LikesCount);
        Assert.Equal(1, second.DislikesCount);

        store.ClearReaction(post.Id, userId);
        var third = store.GetPublic()[0];
        Assert.Equal(0, third.LikesCount);
        Assert.Equal(0, third.DislikesCount);
    }

    [Fact]
    public void ViewerReaction_IsIndependentPerViewer()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var viewer1 = Guid.NewGuid();
        var viewer2 = Guid.NewGuid();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "Per viewer reaction");
        store.SetReaction(post.Id, viewer1, PostReactionType.Like);
        store.SetReaction(post.Id, viewer2, PostReactionType.Dislike);

        var fromViewer1 = store.GetPublic(viewer1)[0];
        var fromViewer2 = store.GetPublic(viewer2)[0];
        var fromNoViewer = store.GetPublic()[0];

        Assert.Equal(PostReactionType.Like, fromViewer1.ViewerReaction);
        Assert.Equal(PostReactionType.Dislike, fromViewer2.ViewerReaction);
        Assert.Null(fromNoViewer.ViewerReaction);
        Assert.Equal(1, fromNoViewer.LikesCount);
        Assert.Equal(1, fromNoViewer.DislikesCount);
    }

    [Fact]
    public void EmptyPostStore_HasNoReactions()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "No reactions");

        Assert.Equal(0, post.LikesCount);
        Assert.Equal(0, post.DislikesCount);
        Assert.Null(post.ViewerReaction);
    }
}


