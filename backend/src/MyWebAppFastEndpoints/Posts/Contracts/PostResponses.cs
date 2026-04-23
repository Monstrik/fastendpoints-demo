public sealed class PublicPostResponse
{
    public Guid Id { get; set; }
    public required string AuthorLogin { get; set; }
    public required string AuthorStatus { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public string? ViewerReaction { get; set; }

    public static PublicPostResponse From(AppPost post) => new()
    {
        Id = post.Id,
        AuthorLogin = post.AuthorLogin,
        AuthorStatus = post.AuthorStatus,
        Content = post.Content,
        CreatedAtUtc = post.CreatedAtUtc,
        LikesCount = post.LikesCount,
        DislikesCount = post.DislikesCount,
        ViewerReaction = post.ViewerReaction?.ToString()
    };
}

public sealed class MyPostResponse
{
    public Guid Id { get; set; }
    public required string AuthorLogin { get; set; }
    public required string AuthorStatus { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsHidden { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public string? ViewerReaction { get; set; }

    public static MyPostResponse From(AppPost post) => new()
    {
        Id = post.Id,
        AuthorLogin = post.AuthorLogin,
        AuthorStatus = post.AuthorStatus,
        Content = post.Content,
        CreatedAtUtc = post.CreatedAtUtc,
        IsHidden = post.IsHidden,
        LikesCount = post.LikesCount,
        DislikesCount = post.DislikesCount,
        ViewerReaction = post.ViewerReaction?.ToString()
    };
}

