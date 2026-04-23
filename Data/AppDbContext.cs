using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PostEntity> Posts => Set<PostEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Login).IsUnique();
            e.Property(u => u.Login).HasMaxLength(100);
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.Property(u => u.Status).HasMaxLength(100).HasDefaultValue(UserStatuses.Default);
        });

        modelBuilder.Entity<PostEntity>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.CreatedAtUtc);
            e.Property(p => p.AuthorLogin).HasMaxLength(100);
            e.Property(p => p.Content).HasMaxLength(280);
        });
    }
}

public class UserEntity
{
    public Guid Id { get; set; }
    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public UserRole Role { get; set; }
    public string Status { get; set; } = UserStatuses.Default;

    public AppUser ToDomain() =>
        new(Id, Login, PasswordHash, FirstName, LastName, Role, Status);

    public static UserEntity FromDomain(AppUser user) => new()
    {
        Id = user.Id,
        Login = user.Login,
        PasswordHash = user.PasswordHash,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role,
        Status = user.Status
    };
}

public class PostEntity
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorLogin { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public bool IsHidden { get; set; }

    public AppPost ToDomain() =>
        new(Id, AuthorId, AuthorLogin, Content, CreatedAtUtc, IsHidden);

    public static PostEntity FromDomain(AppPost post) => new()
    {
        Id = post.Id,
        AuthorId = post.AuthorId,
        AuthorLogin = post.AuthorLogin,
        Content = post.Content,
        CreatedAtUtc = post.CreatedAtUtc,
        IsHidden = post.IsHidden
    };
}

