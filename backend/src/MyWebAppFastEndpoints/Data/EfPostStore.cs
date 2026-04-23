using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

/// <summary>
/// Entity Framework Core implementation of IPostStore with structured logging.
/// </summary>
public sealed class EfPostStore(AppDbContext db) : IPostStore
{
    private static readonly ILogger Logger = Log.ForContext<EfPostStore>();

    public AppPost Create(Guid authorId, string authorLogin, string content)
    {
        try
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

            Logger.Information("Post created: {PostId} by {AuthorLogin}", entity.Id, authorLogin);
            return BuildPosts([entity], authorId).First();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error creating post by {AuthorLogin}", authorLogin);
            throw;
        }
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
        try
        {
            var entity = db.Posts.FirstOrDefault(p => p.Id == id);
            if (entity is null)
            {
                Logger.Warning("Post hide failed: post '{PostId}' not found", id);
                return null;
            }

            entity.IsHidden = true;
            db.SaveChanges();

            Logger.Information("Post hidden: {PostId}", id);
            return GetById(id, null);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error hiding post '{PostId}'", id);
            throw;
        }
    }

    public AppPost? Unhide(Guid id)
    {
        try
        {
            var entity = db.Posts.FirstOrDefault(p => p.Id == id);
            if (entity is null)
            {
                Logger.Warning("Post unhide failed: post '{PostId}' not found", id);
                return null;
            }

            entity.IsHidden = false;
            db.SaveChanges();

            Logger.Information("Post unhidden: {PostId}", id);
            return GetById(id, null);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error unhiding post '{PostId}'", id);
            throw;
        }
    }

    public AppPost? SetReaction(Guid postId, Guid userId, PostReactionType reaction)
    {
        try
        {
            var postExists = db.Posts.Any(p => p.Id == postId);
            if (!postExists)
            {
                Logger.Warning("Reaction set failed: post '{PostId}' not found", postId);
                return null;
            }

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
                Logger.Information("Reaction added: {PostId} by {UserId} ({Reaction})", postId, userId, reaction);
            }
            else
            {
                current.Reaction = reaction;
                Logger.Information("Reaction changed: {PostId} by {UserId} to {Reaction}", postId, userId, reaction);
            }

            db.SaveChanges();

            return GetById(postId, userId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting reaction on post '{PostId}'", postId);
            throw;
        }
    }

    public AppPost? ClearReaction(Guid postId, Guid userId)
    {
        try
        {
            var postExists = db.Posts.Any(p => p.Id == postId);
            if (!postExists)
            {
                Logger.Warning("Reaction clear failed: post '{PostId}' not found", postId);
                return null;
            }

            var current = db.PostReactions.FirstOrDefault(r => r.PostId == postId && r.UserId == userId);
            if (current is not null)
            {
                db.PostReactions.Remove(current);
                db.SaveChanges();
                Logger.Information("Reaction cleared: {PostId} by {UserId}", postId, userId);
            }

            return GetById(postId, userId);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error clearing reaction on post '{PostId}'", postId);
            throw;
        }
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

        var authorIds = entities.Select(p => p.AuthorId).Distinct().ToList();
        var statusByAuthor = db.Users
            .Where(u => authorIds.Contains(u.Id))
            .AsNoTracking()
            .ToDictionary(u => u.Id, u => u.Status);

        return entities
            .Select(p => new AppPost(
                p.Id,
                p.AuthorId,
                p.AuthorLogin,
                statusByAuthor.GetValueOrDefault(p.AuthorId, UserStatuses.Default),
                p.Content,
                p.CreatedAtUtc,
                p.IsHidden,
                likesByPost.GetValueOrDefault(p.Id, 0),
                dislikesByPost.GetValueOrDefault(p.Id, 0),
                viewerByPost.GetValueOrDefault(p.Id)))
            .ToList();
    }
}
