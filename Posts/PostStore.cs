public sealed record AppPost(
    Guid Id,
    Guid AuthorId,
    string AuthorLogin,
    string Content,
    DateTime CreatedAtUtc,
    bool IsHidden);

public interface IPostStore
{
    AppPost Create(Guid authorId, string authorLogin, string content);
    IReadOnlyList<AppPost> GetPublic();
    IReadOnlyList<AppPost> GetByAuthor(Guid authorId);
    AppPost? Hide(Guid id);
}

