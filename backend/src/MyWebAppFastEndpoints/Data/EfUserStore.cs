using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

/// <summary>
/// Entity Framework Core implementation of IUserStore with structured logging.
/// </summary>
public sealed class EfUserStore(AppDbContext db) : IUserStore
{
    private static readonly ILogger Logger = Log.ForContext<EfUserStore>();

    public AppUser? Create(string login, string passwordHash, string firstName, string lastName, UserRole role, string? status = null)
    {
        var trimmed = login.Trim();

        if (db.Users.Any(u => u.Login == trimmed))
        {
            Logger.Warning("User creation failed: login '{Login}' already exists", trimmed);
            return null;
        }

        try
        {
            var entity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Login = trimmed,
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                Status = status ?? UserStatuses.Default
            };

            db.Users.Add(entity);
            db.SaveChanges();
            Logger.Information("User created: {UserId} ({Login})", entity.Id, trimmed);
            return entity.ToDomain();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error creating user '{Login}'", trimmed);
            throw;
        }
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

    public AppUser? Update(Guid id, string login, string? passwordHash, string firstName, string lastName, UserRole role, string? status = null)
    {
        var trimmed = login.Trim();

        try
        {
            var entity = db.Users.FirstOrDefault(u => u.Id == id);
            if (entity is null)
            {
                Logger.Warning("User update failed: user '{UserId}' not found", id);
                return null;
            }

            var loginTaken = db.Users.Any(u => u.Login == trimmed && u.Id != id);
            if (loginTaken)
            {
                Logger.Warning("User update failed: login '{Login}' already taken", trimmed);
                return null;
            }

            entity.Login = trimmed;
            if (!string.IsNullOrWhiteSpace(passwordHash))
                entity.PasswordHash = passwordHash;
            entity.FirstName = firstName;
            entity.LastName = lastName;
            entity.Role = role;
            entity.Status = status ?? entity.Status;

            db.SaveChanges();
            Logger.Information("User updated: {UserId} ({Login})", id, trimmed);
            return entity.ToDomain();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error updating user '{UserId}'", id);
            throw;
        }
    }

    public bool Delete(Guid id)
    {
        try
        {
            var entity = db.Users.FirstOrDefault(u => u.Id == id);
            if (entity is null)
            {
                Logger.Warning("User deletion failed: user '{UserId}' not found", id);
                return false;
            }

            db.Users.Remove(entity);
            db.SaveChanges();
            Logger.Information("User deleted: {UserId} ({Login})", id, entity.Login);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error deleting user '{UserId}'", id);
            throw;
        }
    }
}
