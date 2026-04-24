using System.Security.Claims;
using FastEndpoints;
using MyWebAppFastEndpoints.Shared;

/// <summary>
/// Clears a user's reaction (like or dislike) from a post.
/// </summary>
public sealed class ClearPostReactionEndpoint(IPostStore posts) : Endpoint<PostByIdRequest, PublicPostResponse>
{
    public override void Configure()
    {
        Delete("/api/posts/{id}/reaction");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }

    public override async Task HandleAsync(PostByIdRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var updated = posts.ClearReaction(req.Id, userId.Value);

        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(PublicPostResponse.From(updated), ct);
    }
}
