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
    AppPost? Hide(Guid id);
    AppPost? Unhide(Guid id);
    AppPost? SetReaction(Guid postId, Guid userId, PostReactionType reaction);
    AppPost? ClearReaction(Guid postId, Guid userId);
}

