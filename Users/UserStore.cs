using System.Collections.Concurrent;

public enum UserRole
{
    Admin,
    User
}

public sealed record AppUser(
    Guid Id,
    string Login,
    string PasswordHash,
    string FirstName,
    string LastName,
    int Age,
    UserRole Role);

public interface IUserStore
{
    AppUser? Create(string login, string passwordHash, string firstName, string lastName, int age, UserRole role);
    IReadOnlyList<AppUser> GetAll();
    AppUser? GetById(Guid id);
    AppUser? GetByLogin(string login);
    AppUser? Update(Guid id, string login, string? passwordHash, string firstName, string lastName, int age, UserRole role);
    bool Delete(Guid id);
}

public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<Guid, AppUser> _users = new();
    private readonly ConcurrentDictionary<string, Guid> _idsByLogin = new(StringComparer.OrdinalIgnoreCase);

    public AppUser? Create(string login, string passwordHash, string firstName, string lastName, int age, UserRole role)
    {
        var trimmedLogin = login.Trim();
        var user = new AppUser(Guid.NewGuid(), trimmedLogin, passwordHash, firstName, lastName, age, role);

        if (!_idsByLogin.TryAdd(trimmedLogin, user.Id))
            return null;

        _users[user.Id] = user;
        return user;
    }

    public IReadOnlyList<AppUser> GetAll()
    {
        return _users.Values
            .OrderBy(u => u.Login)
            .ToList();
    }

    public AppUser? GetById(Guid id)
    {
        return _users.TryGetValue(id, out var user) ? user : null;
    }

    public AppUser? GetByLogin(string login)
    {
        if (!_idsByLogin.TryGetValue(login.Trim(), out var id))
            return null;

        return GetById(id);
    }

    public AppUser? Update(Guid id, string login, string? passwordHash, string firstName, string lastName, int age, UserRole role)
    {
        while (true)
        {
            if (!_users.TryGetValue(id, out var current))
                return null;

            var trimmedLogin = login.Trim();
            var existingWithSameLogin = GetByLogin(trimmedLogin);
            if (existingWithSameLogin is not null && existingWithSameLogin.Id != id)
                return null;

            if (!string.Equals(current.Login, trimmedLogin, StringComparison.OrdinalIgnoreCase))
            {
                if (!_idsByLogin.TryAdd(trimmedLogin, id))
                    return null;
            }

            var updated = current with
            {
                Login = trimmedLogin,
                PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? current.PasswordHash : passwordHash,
                FirstName = firstName,
                LastName = lastName,
                Age = age,
                Role = role
            };

            if (_users.TryUpdate(id, updated, current))
            {
                if (!string.Equals(current.Login, trimmedLogin, StringComparison.OrdinalIgnoreCase))
                    _idsByLogin.TryRemove(current.Login, out _);

                return updated;
            }

            if (!string.Equals(current.Login, trimmedLogin, StringComparison.OrdinalIgnoreCase))
                _idsByLogin.TryRemove(trimmedLogin, out _);
        }
    }

    public bool Delete(Guid id)
    {
        if (!_users.TryRemove(id, out var removed))
            return false;

        _idsByLogin.TryRemove(removed.Login, out _);
        return true;
    }
}
