public class EntityMappingTests
{
    [Fact]
    public void UserEntity_ToDomain_MapsAllFields()
    {
        var entity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = "aya",
            PasswordHash = "hash123",
            FirstName = "Aya",
            LastName = "Kovi",
            Role = UserRole.Admin,
            Status = "🎯 Focused"
        };

        var domain = entity.ToDomain();

        Assert.Equal(entity.Id, domain.Id);
        Assert.Equal(entity.Login, domain.Login);
        Assert.Equal(entity.PasswordHash, domain.PasswordHash);
        Assert.Equal(entity.FirstName, domain.FirstName);
        Assert.Equal(entity.LastName, domain.LastName);
        Assert.Equal(entity.Role, domain.Role);
        Assert.Equal(entity.Status, domain.Status);
    }

    [Fact]
    public void UserEntity_FromDomain_MapsAllFields()
    {
        var user = new AppUser(
            Guid.NewGuid(),
            "aya",
            "hash123",
            "Aya",
            "Kovi",
            UserRole.User,
            "🔴 Busy");

        var entity = UserEntity.FromDomain(user);

        Assert.Equal(user.Id, entity.Id);
        Assert.Equal(user.Login, entity.Login);
        Assert.Equal(user.PasswordHash, entity.PasswordHash);
        Assert.Equal(user.FirstName, entity.FirstName);
        Assert.Equal(user.LastName, entity.LastName);
        Assert.Equal(user.Role, entity.Role);
        Assert.Equal(user.Status, entity.Status);
    }

    [Fact]
    public void UserEntity_RoundTrip_ToDomain_FromDomain_IsEqual()
    {
        var original = new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = "roundtrip",
            PasswordHash = "hash",
            FirstName = "Round",
            LastName = "Trip",
            Role = UserRole.User,
            Status = UserStatuses.Default
        };

        var entity = UserEntity.FromDomain(original.ToDomain());

        Assert.Equal(original.Id, entity.Id);
        Assert.Equal(original.Login, entity.Login);
        Assert.Equal(original.PasswordHash, entity.PasswordHash);
        Assert.Equal(original.FirstName, entity.FirstName);
        Assert.Equal(original.LastName, entity.LastName);
        Assert.Equal(original.Role, entity.Role);
        Assert.Equal(original.Status, entity.Status);
    }

    [Fact]
    public void PostEntity_ToDomain_MapsAllFields()
    {
        var entity = new PostEntity
        {
            Id = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            AuthorLogin = "aya",
            Content = "Hello world",
            CreatedAtUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc),
            IsHidden = true
        };

        var domain = entity.ToDomain();

        Assert.Equal(entity.Id, domain.Id);
        Assert.Equal(entity.AuthorId, domain.AuthorId);
        Assert.Equal(entity.AuthorLogin, domain.AuthorLogin);
        Assert.Equal(entity.Content, domain.Content);
        Assert.Equal(entity.CreatedAtUtc, domain.CreatedAtUtc);
        Assert.Equal(entity.IsHidden, domain.IsHidden);
    }

    [Fact]
    public void PostEntity_FromDomain_MapsAllFields()
    {
        var post = new AppPost(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "aya",
            UserStatuses.Default,
            "Hello world",
            new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc),
            IsHidden: false,
            LikesCount: 5,
            DislikesCount: 2);

        var entity = PostEntity.FromDomain(post);

        Assert.Equal(post.Id, entity.Id);
        Assert.Equal(post.AuthorId, entity.AuthorId);
        Assert.Equal(post.AuthorLogin, entity.AuthorLogin);
        Assert.Equal(post.Content, entity.Content);
        Assert.Equal(post.CreatedAtUtc, entity.CreatedAtUtc);
        Assert.Equal(post.IsHidden, entity.IsHidden);
    }

    [Fact]
    public void PostEntity_RoundTrip_ToDomain_FromDomain_IsEqual()
    {
        var original = new PostEntity
        {
            Id = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            AuthorLogin = "roundtrip",
            Content = "Round trip content",
            CreatedAtUtc = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc),
            IsHidden = false
        };

        var entity = PostEntity.FromDomain(original.ToDomain());

        Assert.Equal(original.Id, entity.Id);
        Assert.Equal(original.AuthorId, entity.AuthorId);
        Assert.Equal(original.AuthorLogin, entity.AuthorLogin);
        Assert.Equal(original.Content, entity.Content);
        Assert.Equal(original.CreatedAtUtc, entity.CreatedAtUtc);
        Assert.Equal(original.IsHidden, entity.IsHidden);
    }
}

