using Microsoft.EntityFrameworkCore;

public sealed class EfPostStore(AppDbContext db) : IPostStore
{
    public AppPost Create(Guid authorId, string authorLogin, string content)
    {
        var trimmedContent = content.Trim();

        var entity = new PostEntity
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            AuthorLogin = authorLogin,
            Content = trimmedContent,
            CreatedAtUtc = DateTime.UtcNow,
            IsHidden = false
        };

        db.Posts.Add(entity);
        db.SaveChanges();

        return entity.ToDomain();
    }

    public IReadOnlyList<AppPost> GetPublic() =>
        db.Posts
            .Where(p => !p.IsHidden)
            .OrderByDescending(p => p.CreatedAtUtc)
            .AsNoTracking()
            .Select(p => p.ToDomain())
            .ToList();

    public AppPost? Hide(Guid id)
    {
        var entity = db.Posts.FirstOrDefault(p => p.Id == id);
        if (entity is null)
            return null;

        entity.IsHidden = true;
        db.SaveChanges();

        return entity.ToDomain();
    }
}

