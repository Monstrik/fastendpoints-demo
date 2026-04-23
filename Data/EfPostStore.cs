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

    public IReadOnlyList<AppPost> GetAll(Guid? viewerId = null) =>
        BuildPosts(
            db.Posts
                .OrderByDescending(p => p.CreatedAtUtc)
                .AsNoTracking()
                .ToList(),
            viewerId);

    public IReadOnlyList<AppPost> GetPublic(Guid? viewerId = null) =>
        BuildPosts(
            db.Posts
                .Where(p => !p.IsHidden)
                .OrderByDescending(p => p.CreatedAtUtc)
                .AsNoTracking()
                .ToList(),
            viewerId);

    public IReadOnlyList<AppPost> GetByAuthor(Guid authorId, Guid? viewerId = null) =>
        BuildPosts(
            db.Posts
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAtUtc)
                .AsNoTracking()
                .ToList(),
            viewerId);

    public AppPost? Hide(Guid id)
    {
        var entity = db.Posts.FirstOrDefault(p => p.Id == id);
        if (entity is null)
            return null;

        entity.IsHidden = true;
        db.SaveChanges();

        return entity.ToDomain();
    }

    public AppPost? Unhide(Guid id)
    {
        var entity = db.Posts.FirstOrDefault(p => p.Id == id);
        if (entity is null)
            return null;

        entity.IsHidden = false;
        db.SaveChanges();

        return entity.ToDomain();
    }

    public AppPost? SetReaction(Guid postId, Guid userId, PostReactionType reaction)
    {
        var postExists = db.Posts.Any(p => p.Id == postId);
        if (!postExists)
            return null;

        var current = db.PostReactions.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);

        if (current is null)
        {
            db.PostReactions.Add(new PostReactionEntity
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                Reaction = reaction
            });
        }
        else
        {
            current.Reaction = reaction;
        }

        db.SaveChanges();

        return GetById(postId, userId);
    }

    public AppPost? ClearReaction(Guid postId, Guid userId)
    {
        var postExists = db.Posts.Any(p => p.Id == postId);
        if (!postExists)
            return null;

        var current = db.PostReactions.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);
        if (current is not null)
        {
            db.PostReactions.Remove(current);
            db.SaveChanges();
        }

        return GetById(postId, userId);
    }

    private AppPost? GetById(Guid postId, Guid? viewerId)
    {
        var entity = db.Posts.AsNoTracking().FirstOrDefault(p => p.Id == postId);
        if (entity is null)
            return null;

        return BuildPosts([entity], viewerId).FirstOrDefault();
    }

    private IReadOnlyList<AppPost> BuildPosts(IReadOnlyList<PostEntity> entities, Guid? viewerId)
    {
        if (entities.Count == 0)
            return [];

        var postIds = entities.Select(p => p.Id).ToList();

        var reactions = db.PostReactions
            .Where(r => postIds.Contains(r.PostId))
            .AsNoTracking()
            .ToList();

        var likesByPost = reactions
            .Where(r => r.Reaction == PostReactionType.Like)
            .GroupBy(r => r.PostId)
            .ToDictionary(g => g.Key, g => g.Count());

        var dislikesByPost = reactions
            .Where(r => r.Reaction == PostReactionType.Dislike)
            .GroupBy(r => r.PostId)
            .ToDictionary(g => g.Key, g => g.Count());

        var viewerByPost = viewerId.HasValue
            ? reactions
                .Where(r => r.UserId == viewerId.Value)
                .GroupBy(r => r.PostId)
                .ToDictionary(g => g.Key, g => (PostReactionType?)g.Last().Reaction)
            : new Dictionary<Guid, PostReactionType?>();

        return entities
            .Select(p => new AppPost(
                p.Id,
                p.AuthorId,
                p.AuthorLogin,
                p.Content,
                p.CreatedAtUtc,
                p.IsHidden,
                likesByPost.GetValueOrDefault(p.Id, 0),
                dislikesByPost.GetValueOrDefault(p.Id, 0),
                viewerByPost.GetValueOrDefault(p.Id)))
            .ToList();
    }
}

