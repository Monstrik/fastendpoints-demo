using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

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
    }
}

public class UserEntity
{
    public Guid Id { get; set; }
    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public UserRole Role { get; set; }
    public string Status { get; set; } = UserStatuses.Default;

    public AppUser ToDomain() =>
        new(Id, Login, PasswordHash, FirstName, LastName, Age, Role, Status);

    public static UserEntity FromDomain(AppUser user) => new()
    {
        Id = user.Id,
        Login = user.Login,
        PasswordHash = user.PasswordHash,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Age = user.Age,
        Role = user.Role,
        Status = user.Status
    };
}

