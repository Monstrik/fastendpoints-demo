public sealed class CreatePostRequest
{
    public required string Content { get; set; }
}

public sealed class PostByIdRequest
{
    public Guid Id { get; set; }
}

public sealed class PostReactionRequest
{
    public Guid Id { get; set; }
    public required string Reaction { get; set; }
}

