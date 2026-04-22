using Microsoft.EntityFrameworkCore;

public sealed class EfUserStore(AppDbContext db) : IUserStore
{
    public AppUser? Create(string login, string passwordHash, string firstName, string lastName, int age, UserRole role, string? status = null)
    {
        var trimmed = login.Trim();

        if (db.Users.Any(u => u.Login == trimmed))
            return null;

        var entity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = trimmed,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Age = age,
            Role = role,
            Status = status ?? UserStatuses.Default
        };

        db.Users.Add(entity);
        db.SaveChanges();
        return entity.ToDomain();
    }

    public IReadOnlyList<AppUser> GetAll() =>
        db.Users
            .OrderBy(u => u.Login)
            .AsNoTracking()
            .Select(u => u.ToDomain())
            .ToList();

    public AppUser? GetById(Guid id) =>
        db.Users.AsNoTracking().FirstOrDefault(u => u.Id == id)?.ToDomain();

    public AppUser? GetByLogin(string login)
    {
        var trimmed = login.Trim();
        return db.Users.AsNoTracking().FirstOrDefault(u => u.Login == trimmed)?.ToDomain();
    }

    public AppUser? Update(Guid id, string login, string? passwordHash, string firstName, string lastName, int age, UserRole role, string? status = null)
    {
        var trimmed = login.Trim();

        var entity = db.Users.FirstOrDefault(u => u.Id == id);
        if (entity is null) return null;

        var loginTaken = db.Users.Any(u => u.Login == trimmed && u.Id != id);
        if (loginTaken) return null;

        entity.Login = trimmed;
        if (!string.IsNullOrWhiteSpace(passwordHash))
            entity.PasswordHash = passwordHash;
        entity.FirstName = firstName;
        entity.LastName = lastName;
        entity.Age = age;
        entity.Role = role;
        entity.Status = status ?? entity.Status;

        db.SaveChanges();
        return entity.ToDomain();
    }

    public bool Delete(Guid id)
    {
        var entity = db.Users.FirstOrDefault(u => u.Id == id);
        if (entity is null) return false;

        db.Users.Remove(entity);
        db.SaveChanges();
        return true;
    }
}

