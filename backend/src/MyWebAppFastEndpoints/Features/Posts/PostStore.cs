public enum PostReactionType
{
    Like,
    Dislike
}

public sealed record AppPost(
    Guid Id,
    Guid AuthorId,
    string AuthorLogin,
    string AuthorStatus,
    string Content,
    DateTime CreatedAtUtc,
    bool IsHidden,
    int LikesCount = 0,
    int DislikesCount = 0,
    PostReactionType? ViewerReaction = null);

public interface IPostStore
{
    AppPost Create(Guid authorId, string authorLogin, string content);
    IReadOnlyList<AppPost> GetAll(Guid? viewerId = null);
    IReadOnlyList<AppPost> GetPublic(Guid? viewerId = null);
    IReadOnlyList<AppPost> GetByAuthor(Guid authorId, Guid? viewerId = null);

    /// <summary>
    /// Hides a post.
    /// </summary>
    /// <returns>
    /// Updated post when found; <c>null</c> when the post does not exist.
    /// </returns>
    AppPost? Hide(Guid id);

    /// <summary>
    /// Unhides a post.
    /// </summary>
    /// <returns>
    /// Updated post when found; <c>null</c> when the post does not exist.
    /// </returns>
    AppPost? Unhide(Guid id);

    /// <summary>
    /// Sets or replaces the current user's reaction on a post.
    /// </summary>
    /// <returns>
    /// Updated post when found; <c>null</c> when the post does not exist.
    /// </returns>
    AppPost? SetReaction(Guid postId, Guid userId, PostReactionType reaction);

    /// <summary>
    /// Clears the current user's reaction from a post.
    /// </summary>
    /// <returns>
    /// Updated post when found; <c>null</c> when the post does not exist.
    /// </returns>
    AppPost? ClearReaction(Guid postId, Guid userId);
}

