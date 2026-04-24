public sealed class PostNotFoundException(Guid postId)
    : Exception($"Post '{postId}' was not found.")
{
    public Guid PostId { get; } = postId;
}

