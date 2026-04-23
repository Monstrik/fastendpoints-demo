using Microsoft.EntityFrameworkCore;

public class EfPostStoreTests
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
    public void Create_WithValidData_CreatesPost()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "Test content");

        Assert.NotNull(post);
        Assert.Equal("Test content", post.Content);
        Assert.Equal(admin.Id, post.AuthorId);
        Assert.Equal(admin.Login, post.AuthorLogin);
        Assert.False(post.IsHidden);
        Assert.Equal(0, post.LikesCount);
        Assert.Equal(0, post.DislikesCount);
        Assert.Null(post.ViewerReaction);
    }

    [Fact]
    public void Create_TrimsContent()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "  Content with spaces  ");

        Assert.Equal("Content with spaces", post.Content);
    }

    [Fact]
    public void GetAll_ReturnsAllPosts_OrderedByCreatedDescending()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post1 = store.Create(admin.Id, admin.Login, "First");
        System.Threading.Thread.Sleep(10);
        var post2 = store.Create(admin.Id, admin.Login, "Second");
        System.Threading.Thread.Sleep(10);
        var post3 = store.Create(admin.Id, admin.Login, "Third");

        var all = store.GetAll();

        Assert.Equal(3, all.Count);
        Assert.Equal(post3.Id, all[0].Id);
        Assert.Equal(post2.Id, all[1].Id);
        Assert.Equal(post1.Id, all[2].Id);
    }

    [Fact]
    public void GetPublic_ExcludesHiddenPosts()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var visible = store.Create(admin.Id, admin.Login, "Visible");
        var hidden = store.Create(admin.Id, admin.Login, "Hidden");
        store.Hide(hidden.Id);

        var publicPosts = store.GetPublic();

        Assert.Single(publicPosts);
        Assert.Equal(visible.Id, publicPosts[0].Id);
    }

    [Fact]
    public void GetByAuthor_ReturnsOnlyAuthorPosts()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();

        var author2 = new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = "author2",
            PasswordHash = "hash",
            FirstName = "Author",
            LastName = "Two",
            Role = UserRole.User,
            Status = UserStatuses.Default
        };
        db.Users.Add(author2);
        db.SaveChanges();

        var store = new EfPostStore(db);
        var adminPost = store.Create(admin.Id, admin.Login, "Admin post");
        var authorPost = store.Create(author2.Id, author2.Login, "Author post");

        var authorPosts = store.GetByAuthor(author2.Id);

        Assert.Single(authorPosts);
        Assert.Equal(authorPost.Id, authorPosts[0].Id);
    }

    [Fact]
    public void Hide_HidesPost()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "Hide me");
        var hidden = store.Hide(post.Id);

        Assert.NotNull(hidden);
        Assert.True(hidden!.IsHidden);

        var publicPosts = store.GetPublic();
        Assert.DoesNotContain(publicPosts, p => p.Id == post.Id);
    }

    [Fact]
    public void Unhide_UnhidesPost()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "Unhide me");
        store.Hide(post.Id);
        var unhidden = store.Unhide(post.Id);

        Assert.NotNull(unhidden);
        Assert.False(unhidden!.IsHidden);

        var publicPosts = store.GetPublic();
        Assert.Contains(publicPosts, p => p.Id == post.Id);
    }

    [Fact]
    public void Hide_WhenPostDoesNotExist_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfPostStore(db);

        var result = store.Hide(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void Unhide_WhenPostDoesNotExist_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfPostStore(db);

        var result = store.Unhide(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void SetReaction_AddsLike()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var userId = Guid.NewGuid();

        var store = new EfPostStore(db);
        var post = store.Create(admin.Id, admin.Login, "Like me");
        var reacted = store.SetReaction(post.Id, userId, PostReactionType.Like);

        Assert.NotNull(reacted);
        Assert.Equal(1, reacted!.LikesCount);
        Assert.Equal(0, reacted.DislikesCount);
        Assert.Equal(PostReactionType.Like, reacted.ViewerReaction);
    }

    [Fact]
    public void SetReaction_AddsDislike()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var userId = Guid.NewGuid();

        var store = new EfPostStore(db);
        var post = store.Create(admin.Id, admin.Login, "Dislike me");
        var reacted = store.SetReaction(post.Id, userId, PostReactionType.Dislike);

        Assert.NotNull(reacted);
        Assert.Equal(0, reacted!.LikesCount);
        Assert.Equal(1, reacted.DislikesCount);
        Assert.Equal(PostReactionType.Dislike, reacted.ViewerReaction);
    }

    [Fact]
    public void SetReaction_ChangesReaction()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var userId = Guid.NewGuid();

        var store = new EfPostStore(db);
        var post = store.Create(admin.Id, admin.Login, "React me");
        
        store.SetReaction(post.Id, userId, PostReactionType.Like);
        var changed = store.SetReaction(post.Id, userId, PostReactionType.Dislike);

        Assert.NotNull(changed);
        Assert.Equal(0, changed!.LikesCount);
        Assert.Equal(1, changed.DislikesCount);
        Assert.Equal(PostReactionType.Dislike, changed.ViewerReaction);
    }

    [Fact]
    public void SetReaction_WhenPostDoesNotExist_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfPostStore(db);

        var result = store.SetReaction(Guid.NewGuid(), Guid.NewGuid(), PostReactionType.Like);

        Assert.Null(result);
    }

    [Fact]
    public void ClearReaction_RemovesReaction()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var userId = Guid.NewGuid();

        var store = new EfPostStore(db);
        var post = store.Create(admin.Id, admin.Login, "Clear me");
        store.SetReaction(post.Id, userId, PostReactionType.Like);
        var cleared = store.ClearReaction(post.Id, userId);

        Assert.NotNull(cleared);
        Assert.Equal(0, cleared!.LikesCount);
        Assert.Null(cleared.ViewerReaction);
    }

    [Fact]
    public void ClearReaction_WhenPostDoesNotExist_ReturnsNull()
    {
        var db = CreateDbContext();
        var store = new EfPostStore(db);

        var result = store.ClearReaction(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void ClearReaction_WhenNoReaction_StillReturnsPost()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var store = new EfPostStore(db);

        var post = store.Create(admin.Id, admin.Login, "No reaction");
        var result = store.ClearReaction(post.Id, Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal(post.Id, result!.Id);
    }

    [Fact]
    public void GetPublic_WithViewerId_IncludesViewerReaction()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var viewerId = Guid.NewGuid();

        var store = new EfPostStore(db);
        var post = store.Create(admin.Id, admin.Login, "View me");
        store.SetReaction(post.Id, viewerId, PostReactionType.Like);

        var publicPosts = store.GetPublic(viewerId);
        var retrieved = publicPosts.First(p => p.Id == post.Id);

        Assert.Equal(PostReactionType.Like, retrieved.ViewerReaction);
    }

    [Fact]
    public void GetPublic_WithoutViewerId_ExcludesViewerReaction()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();
        var viewerId = Guid.NewGuid();

        var store = new EfPostStore(db);
        var post = store.Create(admin.Id, admin.Login, "View me");
        store.SetReaction(post.Id, viewerId, PostReactionType.Like);

        var publicPosts = store.GetPublic();
        var retrieved = publicPosts.First(p => p.Id == post.Id);

        Assert.Null(retrieved.ViewerReaction);
    }

    [Fact]
    public void MultipleReactions_CountsCorrectly()
    {
        var db = CreateDbContext();
        SeedAdminUser(db);
        var admin = db.Users.First();

        var store = new EfPostStore(db);
        var post = store.Create(admin.Id, admin.Login, "Multiple reactions");

        store.SetReaction(post.Id, Guid.NewGuid(), PostReactionType.Like);
        store.SetReaction(post.Id, Guid.NewGuid(), PostReactionType.Like);
        store.SetReaction(post.Id, Guid.NewGuid(), PostReactionType.Dislike);

        var retrieved = store.GetPublic()[0];

        Assert.Equal(2, retrieved.LikesCount);
        Assert.Equal(1, retrieved.DislikesCount);
    }
}

