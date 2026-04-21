using System.Collections.Concurrent;

public sealed record InMemoryUser(Guid Id, string FirstName, string LastName, int Age);

public interface IUserStore
{
    InMemoryUser Create(string firstName, string lastName, int age);
    IReadOnlyList<InMemoryUser> GetAll();
    InMemoryUser? GetById(Guid id);
    InMemoryUser? Update(Guid id, string firstName, string lastName, int age);
    bool Delete(Guid id);
}

public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<Guid, InMemoryUser> _users = new();

    public InMemoryUser Create(string firstName, string lastName, int age)
    {
        var user = new InMemoryUser(Guid.NewGuid(), firstName, lastName, age);
        _users[user.Id] = user;
        return user;
    }

    public IReadOnlyList<InMemoryUser> GetAll()
    {
        return _users.Values
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();
    }

    public InMemoryUser? GetById(Guid id)
    {
        return _users.TryGetValue(id, out var user) ? user : null;
    }

    public InMemoryUser? Update(Guid id, string firstName, string lastName, int age)
    {
        while (true)
        {
            if (!_users.TryGetValue(id, out var current))
                return null;

            var updated = current with { FirstName = firstName, LastName = lastName, Age = age };

            if (_users.TryUpdate(id, updated, current))
                return updated;
        }
    }

    public bool Delete(Guid id)
    {
        return _users.TryRemove(id, out _);
    }
}

